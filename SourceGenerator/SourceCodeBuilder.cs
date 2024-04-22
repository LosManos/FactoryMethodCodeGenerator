using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal class SourceCodeBuilder(){

    internal string Build(
        IList<string> output,
        IMethodSymbol mainMethod)
    {
        string source = $@"// <auto-generated/>
// {DateTime.Now:O}
using System;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public static partial class {mainMethod.ContainingType.Name}
    {{
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{{name}}!!!'"");
    }}
}}
";
        return source;
    }

    internal IEnumerable<string> Build(
        Compilation compilation,
        IImmutableList<(TypeCollector.IsOfType isOfType, TypeDeclarationSyntax type, IEnumerable<AttributeSyntax>
            attribs)> types)
    {
        foreach (var type in types)
        {
            yield return Build(compilation, type.isOfType, type.type, type.attribs);
        }
    }

    private string Build(
        Compilation compilation,
        TypeCollector.IsOfType isOfType,
        TypeDeclarationSyntax type,
        IEnumerable<AttributeSyntax> attribs)
    {
        // Which model is this?
        // var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Skip(0).First());

        ClassDeclarationSyntax CreateClass(string name) =>
            SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(name))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        var defaultConstructor = SyntaxFactory.ConstructorDeclaration(
                "MyRecord")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBody(SyntaxFactory.Block());

        RecordDeclarationSyntax CreateRecord(RecordInfo recordInfo)
        {
            var propertiesAsString = string.Join(",", recordInfo.Properties.Select(p => p.Text));

            var constructorInfo = ConstructorInfo.Create(recordInfo.Name, recordInfo.Properties);

            var constructor = CreateConstructor(constructorInfo);

            var res = SyntaxFactory.RecordDeclaration(
                    SyntaxKind.RecordDeclaration,
                    SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                    SyntaxFactory.Identifier(recordInfo.Name))
                .WithModifiers(SyntaxTokenList.Create(
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                ))
                .WithLeadingTrivia(CreateSingleLineComment($"Properties: {propertiesAsString}"))
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
                .AddMembers(constructor);
            return res;
        }

        var autoComment = SyntaxFactory.Comment("// <auto-generated/>");
        var nsComments = new List<SyntaxTrivia>();

        var nameSpaceName = GetNamespace(type);
        var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpaceName)).NormalizeWhitespace();


        nsComments.Add(SyntaxFactory.Comment($"// Namespace for method:{nameSpaceName}"));

        // var className = type.type.GetDeclaredSymbol(compilation)?.ToDisplayString();
        if (isOfType == TypeCollector.IsOfType.IsClass)
        {
            var className = type.GetDeclaredSymbol(compilation)?.Name.ToString();
            ns = ns.AddMembers(CreateClass(className));
        }

        if (isOfType == TypeCollector.IsOfType.IsRecord)
        {
            var symbol = type.GetDeclaredSymbol(compilation);

            var members = type.Members.Select(m => m as PropertyDeclarationSyntax).Where(m => m is not null);

            var recordName = type.GetDeclaredSymbol(compilation)?.Name;

            var recordInfo = RecordInfo.Create(recordName,
                members.Select(PropertyInfo.Create));

            ns = ns.AddMembers(CreateRecord(recordInfo));
        }

        string source = $@"{autoComment} 
// {DateTime.Now:O}

// classes:
// {"output.StringJoinNL()"}
// namespace:
// {nsComments.Select(c => c.ToString()).StringJoinNL()}

{ns.NormalizeWhitespace()}
";

        return source;
    }

    private static SyntaxTriviaList CreateSingleLineComment(string comment)
    {
        // Future: Here is how to write xml comment. https://stackoverflow.com/questions/30695752/how-do-i-add-an-xml-doc-comment-to-a-classdeclarationsyntax-in-roslyn
        return SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// "+ comment));
    }

    /// <summary>Create a constructor taking a list of parameters
    /// and updating properties of the same name.
    /// </summary>
    /// <param name="constructorInfo"></param>
    /// <returns></returns>
    private static ConstructorDeclarationSyntax CreateConstructor(ConstructorInfo constructorInfo)
    {
        var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var parameters = CreateParameterList(constructorInfo.Properties);

        var body = SyntaxFactory.Block(constructorInfo.Properties.Select(propertyInfo =>
            // Assignment. E.g.: this.MyProperty = MyProperty;
            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName("this." + propertyInfo.Name),
                SyntaxFactory.IdentifierName(propertyInfo.Name)))
        ));

        var ret = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(constructorInfo.Name))
            .WithModifiers(modifiers)
            .WithParameterList(parameters)
            .WithBody(body);
        return ret;
    }

    private static ParameterListSyntax CreateParameterList(IEnumerable<PropertyInfo> propertyInfos)
    {
        var parameters = SyntaxFactory.ParameterList();
        foreach (var propertyInfo in propertyInfos)
        {
            parameters = parameters.AddParameters(CreateParameter(propertyInfo));
        }
        return parameters;
    }

    private static ParameterSyntax CreateParameter(PropertyInfo propertyInfo)
    {
        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(propertyInfo.Name))
            .WithType(propertyInfo.Type);
    }

    /// <summary> Copied with pride from https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/
    // determine the namespace the class/enum/struct is declared in, if any
    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }
}