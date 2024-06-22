using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGenerator;

partial class SourceCodeBuilder
{
    public (string source, string namespaceName, string recordName) BuildMapRecord(
        SourceProductionContext spc,
        RecordDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);

        var record = CreateMapRecord(spc, syntax);

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
        RecordDeclarationSyntax syntax)
    {
        var methodModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword));
        var recordName = syntax.Identifier.Text;

        var mapAttributes = syntax.AttributeLists.GetMapAttributes()
            .Select(a => (name: a.Name, sourceType: a.ArgumentList?.Arguments.First()));

        var mapFunctionSourceParameterName = "source";

        var methods = mapAttributes.Select(attrib =>
                MethodDeclaration(
                    ParseTypeName(GetTargetTypeName(attrib.name)),
                    GetSourceTypeName(attrib.name) + "_To_" + GetTargetTypeName(attrib.name))
                    .WithModifiers(methodModifiers)
                    .WithParameterList(CreateCopyFunctionParameter(attrib.name, mapFunctionSourceParameterName))
                    .WithBody(Block(CreateBody(mapAttributes, mapFunctionSourceParameterName)))
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
        static StatementSyntax CreateBody(IEnumerable<(NameSyntax name, AttributeArgumentSyntax? sourceType)> attributeData, string parameterName)
        {
            const string resultVariableName = "result";

            // TODO:OF:This is a simple hard coded array. Get the proper properties.
            (Type theType, string name)[] argumentsData = [(typeof(int), "Value")];

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
    }
}