using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGenerator;

partial class SourceCodeBuilder
{
    public (string source, string namespaceName, string recordName) BuildMapRecord(
        SourceProductionContext spc,
        SemanticModel semanticModel,
        RecordDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);

        var record = CreateMapRecord(spc, semanticModel, syntax);

        var namespaceDeclaration =
            NamespaceDeclaration(ParseName(@namespace.Name.ToString()))
                .AddMembers(record);

        var unit = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddMembers(namespaceDeclaration);

//        var sourceCode = $"public record RemoveMe;";

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

    /// <summary>Creates the record containing the mapping methods.
    /// Crude example:
    ///     public abstract partial record Mapping
    ///     {
    ///         public static CopyTarget CopySource_To_CopyTarget(CopySource source)
    ///         {
    ///             return CopyTarget.Create(source.Id, source.Name);
    ///         }
    ///     }
    /// </summary>
    private static RecordDeclarationSyntax CreateMapRecord(
        SourceProductionContext spc,
        SemanticModel semanticModel,
        RecordDeclarationSyntax syntax)
    {
        var methodModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword));
        var recordName = syntax.Identifier.Text;

        var mapAttributes = syntax.AttributeLists.GetMapAttributes()
            .Select(a => (
                name: a.Name,   // Name of the attribute. Something line `Map<SourceType,TargetType>`.
                sourceType: a.ArgumentList?.Arguments.First())  // The first argument. Something like `SourceType`.
            );

        var mapFunctionSourceParameterName = "source";

        var methods = mapAttributes.Select(attrib =>
                MethodDeclaration(
                    ParseTypeName(GetTargetTypeName(attrib.name)),
                    GetSourceTypeName(attrib.name) + "_To_" + GetTargetTypeName(attrib.name))
                    .WithModifiers(methodModifiers)
                    .WithParameterList(CreateCopyFunctionParameter(attrib.name, mapFunctionSourceParameterName))
                    .WithBody(Block(CreateBody(spc, semanticModel, mapAttributes, mapFunctionSourceParameterName)))
                )
            .ToArray();

        var method = MethodDeclaration(
            ParseTypeName("void"),
            "MyMethodName");

        var recordModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.AbstractKeyword),
            Token(SyntaxKind.PartialKeyword)
        );

        var res = RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                Token(SyntaxKind.RecordKeyword),
                Identifier(recordName))
            .WithModifiers(recordModifiers)
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
            .AddMembers(methods);

        return res;

        static ParameterListSyntax CreateCopyFunctionParameter(NameSyntax ns, string sourceParameterName)
        {
            var parameters = ParameterList()
                .AddParameters(
                    CreateParameter(
                        PropertyInfo.Create(
                            sourceParameterName,
                            IdentifierName(Identifier(GetSourceTypeName(ns))),
                            string.Empty)));
            return parameters;
        }

        static string GetSourceTypeName(NameSyntax ns)
        {
            var gns = (ns as GenericNameSyntax);
            var arg = gns?.TypeArgumentList.Arguments[0];
            var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
            return ins ?? throw new Exception($"Error in {nameof(GetSourceTypeName)}.");
        }

        static string GetTargetTypeName(NameSyntax ns)
        {
            var gns = (ns as GenericNameSyntax);
            var arg = gns?.TypeArgumentList.Arguments[1];
            var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
            return ins ?? throw new Exception($"Error in {nameof(GetSourceTypeName)}.");
        }

        // Creates a body like
        // {
        //     return CopyTarget.Create(source.Id, source.Name);
        // }
        static StatementSyntax CreateBody(
            SourceProductionContext sourceProductionContext,
            SemanticModel semanticModel,
            IEnumerable<(NameSyntax name, AttributeArgumentSyntax? sourceType)> attributeData,
            string parameterName)
        {
            const string resultVariableName = "result";

            // TODO:OF:This is a simple hard coded array. Get the proper properties.
            // Something like attributeData.sourceType..getProperties
            (Type theType, string name)[] argumentsData = [(typeof(int), "Value")];

            var attribute = attributeData.First().sourceType.FirstAncestorOrSelf<AttributeSyntax>()
                ?? throw new Exception("Could not find the target attribute.");
            // var args = attribute.ArgumentList.Arguments;

            var targetProperties  = GetTargetProperties(attribute, semanticModel);

            var memberAccessArguments = argumentsData.Select(ad =>
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(parameterName),
                        IdentifierName(ad.name)
                    ))
                .Select(Argument);
            var memberAccessArgumentsList = SeparatedList<ArgumentSyntax>(memberAccessArguments);

            var createCall =
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( GetTargetTypeName(attributeData.Single().name)),
                            IdentifierName("Create")))
                    .WithArgumentList(
                        ArgumentList(memberAccessArgumentsList));

            // var result = CopyTarget.Create(...
            var assignment = ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName($"var {resultVariableName}"),
                createCall
            ));

            var body = Block(
                assignment
            );
            return body.AddStatements(ParseStatement($"return {resultVariableName};"));
        }

        ArgumentSyntax CreateLiteralArgument(int value)
        {
            return Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(value)));
        }

        static IEnumerable<(ITypeSymbol Type, string Name)> GetTargetProperties(AttributeSyntax attribute, SemanticModel semanticModel1)
        {
            if (attribute.Name is GenericNameSyntax genericNameSyntax)
            {
                var typeArguments = genericNameSyntax.TypeArgumentList.Arguments;
                // First argument is Source, second is Target.
                var arg = typeArguments[1];
                var info = semanticModel1.GetTypeInfo(arg);

                if (info.Type is INamedTypeSymbol namedTypeSymbol)
                {
                    var properties = namedTypeSymbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                        .Select(p => (p.Type, p.Name));
                    return properties;
                }
            }
            throw new Exception("Something went wrong when trying to find the argument for target.");
        }
    }
}