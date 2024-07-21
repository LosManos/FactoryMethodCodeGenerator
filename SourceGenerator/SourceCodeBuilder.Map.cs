using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGenerator;

internal static class SourceCodeBuilderMap
{
    /// <summary>Creates a method for copying all data from one class to another.
    ///
    /// A Build...-method takes whatever Roslyn-ish is needed
    /// and distills the needed data for creating the result
    /// with as little as Roslyn knowledge as possible.
    /// So avoid passing <see cref="SourceProductionContext"/>, <see cref="SemanticModel"/> and <see cref="ClassDeclarationSyntax"/> along.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public static (string source, string namespaceName, string recordName) BuildMapRecord(
        (SourceProductionContext _, SemanticModel semanticModel) context,
        RecordDeclarationSyntax syntax)
    {
        var namespaceName = SyntaxHelper.GetNameSpaceName(syntax);
        // The name of the record containing the mapping methods.
        // E.g.: `public abstract partial record Mapping` where this is "Mapping".
        var recordName = SyntaxHelper.GetRecordNameString(syntax);
        var mappingInfo = GetMappingInformation(recordName, syntax.AttributeLists.GetMapAttributes());

        // The name of the parameter for the mapping function.
        // E.g.: `Source_To_Target(Source source)` where this is "source".
        var mapFunctionSourceParameterName = "source";

        var methods = mappingInfo.Infos
            .Select(attrib =>
                {
                    var attributeName = attrib.SourceType?.FirstAncestorOrSelf<AttributeSyntax>()?.Name
                                        ?? throw new Exception("Could not find the target attribute.");

                    if (attributeName is GenericNameSyntax genericNameSyntax)
                    {
                        var typeArguments = genericNameSyntax.TypeArgumentList.Arguments;
                        var targetTypeArgument = typeArguments[1]; // First argument is Source, second is Target.
                        var targetTypeInfo = context.semanticModel.GetTypeInfo(targetTypeArgument);

                        return CreateMappingMethod(attrib.SourceTypeName,
                            attrib.TargetTypeName,
                            targetTypeInfo,
                            mapFunctionSourceParameterName);
                    }

                    throw new Exception("Something went wrong when trying to find the argument for target.");
                }
            );

        var record = CreateMapRecord(recordName, methods);

        var namespaceDeclaration =
            NamespaceDeclaration(ParseName(namespaceName))
                .AddMembers(record);

        var unit = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddMembers(namespaceDeclaration);

        return (unit.NormalizeWhitespace().ToFullString(), namespaceName, recordName);
    }

    /// <summary>Creates the record containing the mapping methods.
    /// Crude example:
    ///     public abstract partial record Mapping
    ///     {
    ///         public static Target Source_To_Target(Source source)
    ///         {
    ///             return Target.Create(source.Id, source.Name);
    ///         }
    ///     }
    /// </summary>
    private static RecordDeclarationSyntax CreateMapRecord(
        string recordName,
        IEnumerable<MemberDeclarationSyntax> methods)
    {
        return RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                Token(SyntaxKind.RecordKeyword),
                Identifier(recordName))
            .WithModifiers(CreateMappingRecordModifiers())
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
            .AddMembers(methods.ToArray());
    }

    private static MappingInfos GetMappingInformation(string recordName, IEnumerable<AttributeSyntax> attributes)
    {
        var mapAttributes = attributes.GetMapAttributes()
            .Select(a => (
                // Name of the attribute. Something line `Map<SourceType,TargetType>`.
                sourceTypeName: GetMappingSourceTypeName(a.Name),
                targetTypeName: GetMappingTargetTypeName(a.Name),
                sourceType: a.ArgumentList?.Arguments.Skip(1).First())
            );

        return MappingInfos.Create(
            recordName,
            mapAttributes.Select(ma => MappingInfo.Create(
                "", // TODO:OF:What is this?
                ma.sourceTypeName,
                ma.targetTypeName,
                ma.sourceType)));
    }

    private static IEnumerable<string> GetTargetPropertyNames(TypeInfo targetTypeInfo)
    {
        if (targetTypeInfo.Type is INamedTypeSymbol namedTypeSymbol)
        {
            return GetPublicPropertyNames(namedTypeSymbol);
        }

        throw new Exception("Something went wrong when trying to find the argument name for target.");
    }

    /// <summary>Returns ["A", "B"] for
    /// public record MyRecord {
    ///     public int A {get;init;}
    ///     public string B {get;init;}
    /// }
    /// </summary>
    /// <param name="namedTypeSymbol"></param>
    /// <returns></returns>
    private static IEnumerable<string> GetPublicPropertyNames(INamedTypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public)
            .Select(p => p.Name);
    }

    // `source.A, source.B, source.C`
    private static ParameterListSyntax CreateCopyFunctionParameter(
        string sourceTypeName,
        string sourceParameterName)
    {
        return ParameterList()
            .AddParameters(
                SourceCodeBuilder.CreateParameter(
                    PropertyInfo.Create(
                        sourceParameterName,
                        IdentifierName(Identifier(sourceTypeName)),
                        string.Empty)));
    }

    /// <summary> Creates a body like
    /// ```
    /// {
    ///     return CopyTarget.Create(source.Id, source.Name);
    /// }
    /// ```
    /// </summary>
    /// <param name="targetAttributeName"></param>
    /// <param name="targetTypeInfo"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    static BlockSyntax CreateBody(
        string targetAttributeName,
        TypeInfo targetTypeInfo,
        string parameterName)
    {
        const string resultVariableName = "result";
        const string factoryMethodName = "Create";

        var targetPropertyNames = GetTargetPropertyNames(targetTypeInfo).ToArray();

        var memberAccessArgumentsList = GenerateParametersForFactoryMethodCall(targetPropertyNames, parameterName);

        // `Target.Create(source.A, source.B...);`
        var createCall =
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(targetAttributeName),
                        IdentifierName(factoryMethodName)))
                .WithArgumentList(
                    ArgumentList(memberAccessArgumentsList));

        // `var result = Target.Create(...`
        var assignment = ExpressionStatement(AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName($"var {resultVariableName}"),
            createCall
        ));

        var body = Block(
            CreateComment("Copy properties from Source to Target."),
            CreateCopyCommentComment(targetPropertyNames),
            assignment,
            ParseStatement($"return {resultVariableName};")
        );
        return body;
    }

    private static StatementSyntax CreateCopyCommentComment(IEnumerable<string> targetPropertyNames)
    {
        return CreateComment($"From Target we are copying fields [{string.Join(", ", targetPropertyNames)}].");
    }

    private static StatementSyntax CreateComment(string commentText)
    {
        return ParseStatement("").WithLeadingTrivia(
            Comment($"// {commentText}"));
    }

    private static MemberDeclarationSyntax CreateMappingMethod(string sourceTypeName,
        string targetTypeName,
        TypeInfo targetTypeInfo,
        string mapFunctionSourceParameterName)
    {
        var methodModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword));

        return MethodDeclaration(
                ParseTypeName(targetTypeName),
                sourceTypeName + "_To_" + targetTypeName)
            .WithModifiers(methodModifiers)
            .WithParameterList(CreateCopyFunctionParameter(sourceTypeName, mapFunctionSourceParameterName))
            .WithBody(
                CreateBody(
                    targetTypeName,
                    targetTypeInfo,
                    mapFunctionSourceParameterName
                )
            );
    }

    private static SyntaxTokenList CreateMappingRecordModifiers()
    {
        var recordModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.AbstractKeyword),
            Token(SyntaxKind.PartialKeyword)
        );
        return recordModifiers;
    }

    private static SeparatedSyntaxList<ArgumentSyntax> GenerateParametersForFactoryMethodCall(
        IEnumerable<string> targetPropertyNames,
        string parameterName
    )
    {
        var memberAccessArguments = targetPropertyNames.Select(tpn =>
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(parameterName),
                    IdentifierName(tpn)
                ))
            .Select(Argument);
        var memberAccessArgumentsList = SeparatedList(memberAccessArguments);
        return memberAccessArgumentsList;
    }

    private static string GetMappingSourceTypeName(NameSyntax ns)
    {
        var gns = (ns as GenericNameSyntax);
        var arg = gns?.TypeArgumentList.Arguments[0];
        var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
        return ins ?? throw new Exception($"Error in {nameof(GetMappingSourceTypeName)}.");
    }

    private static string GetMappingTargetTypeName(NameSyntax ns)
    {
        var gns = (ns as GenericNameSyntax);
        var arg = gns?.TypeArgumentList.Arguments[1];
        var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
        return ins ?? throw new Exception($"Error in {nameof(GetMappingTargetTypeName)}.");
    }
}