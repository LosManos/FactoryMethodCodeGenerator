using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyInterface;

namespace SourceGenerator;

internal class SourceCodeBuilder
{
    public ( string source, string namespaceName, string recordName) BuildClass(
        SourceProductionContext spc,
        ClassDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);
        var members = GetProperties(syntax);
        var usePrivateConstructor = GetUsePrivateConstructor(syntax.AttributeLists);

        var @class = CreateClass(RecordOrClassInfo.Create(
            syntax.Identifier.Text,
            members.Select(PropertyInfo.Create),
            usePrivateConstructor));

        var namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace.Name.ToString()))
                .AddMembers(@class);

        var unit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration);

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

    public (string source, string namespaceName, string recordName) BuildRecord(
        SourceProductionContext spc,
        RecordDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);
        var members = GetProperties(syntax);
        var usePrivateConstructor = GetUsePrivateConstructor(syntax.AttributeLists);

        var record = CreateRecord(RecordOrClassInfo.Create(
            syntax.Identifier.Text,
            members.Select(PropertyInfo.Create),
            usePrivateConstructor));

        var namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace.Name.ToString()))
                .AddMembers(record);

        var unit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration);

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

    private static ArgumentSyntax CreateArgument(PropertyInfo propertyInfo)
    {
        return SyntaxFactory.Argument(SyntaxFactory.IdentifierName(propertyInfo.Name));
    }

    private static ArgumentListSyntax CreateArgumentList(IEnumerable<PropertyInfo> propertyInfos)
    {
        var arguments = SyntaxFactory.ArgumentList();
        foreach (var propertyInfo in propertyInfos)
        {
            arguments = arguments.AddArguments(CreateArgument(propertyInfo));
        }
        return arguments;
    }

    private ClassDeclarationSyntax CreateClass(RecordOrClassInfo recordOrClassInfo)
    {
        var constructorInfo = ConstructorInfo.Create(recordOrClassInfo.Name, recordOrClassInfo.Properties, recordOrClassInfo.IsPrivateConstructor);
        var constructor = CreateConstructor(constructorInfo);

        var factoryMethod = CreateFactoryMethod(constructorInfo);

        return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(recordOrClassInfo.Name))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(constructor, factoryMethod);
    }

    /// <summary>Create a constructor taking a list of parameters
    /// and updating properties of the same name.
    /// </summary>
    /// <param name="constructorInfo"></param>
    /// <returns></returns>
    private static ConstructorDeclarationSyntax CreateConstructor(ConstructorInfo constructorInfo)
    {
        var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(constructorInfo.IsPrivateConstructor
            ? SyntaxKind.PrivateKeyword
            : SyntaxKind.PublicKeyword));

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

    private static MethodDeclarationSyntax CreateFactoryMethod(ConstructorInfo constructorInfo)
    {
        var modifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        var parameters = CreateParameterList(constructorInfo.Properties);
        var arguments = CreateArgumentList(constructorInfo.Properties);

        var body = SyntaxFactory.Block(
            // Return. E.g.: return new MyDto(a,b,c);
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(constructorInfo.Name)
                ).WithArgumentList(arguments)
            ));

        var ret = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(constructorInfo.Name),
                "Create")
            .WithModifiers(modifiers)
            .WithParameterList(parameters)
            .WithBody(body);
        return ret;
    }

    private static NamespaceDeclarationSyntax CreateNamespace(string nameSpaceName)
    {
        return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpaceName)).NormalizeWhitespace();
    }

    private static ParameterSyntax CreateParameter(PropertyInfo propertyInfo)
    {
        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(propertyInfo.Name))
            .WithType(propertyInfo.Type);
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

    private static RecordDeclarationSyntax CreateRecord(RecordOrClassInfo recordOrClassInfo)
    {
        var propertiesAsString = string.Join(",", recordOrClassInfo.Properties.Select(p => p.Text));

        var constructorInfo = ConstructorInfo.Create(recordOrClassInfo.Name, recordOrClassInfo.Properties, recordOrClassInfo.IsPrivateConstructor);

        var constructor = CreateConstructor(constructorInfo);

        var factoryMethod = CreateFactoryMethod(constructorInfo);

        var res = SyntaxFactory.RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                SyntaxFactory.Identifier(recordOrClassInfo.Name))
            .WithModifiers(SyntaxTokenList.Create(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            ))
            .WithLeadingTrivia(CreateSingleLineComment($"Properties: {propertiesAsString}"))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
            .AddMembers(constructor, factoryMethod);
        return res;
    }

    private static SyntaxTriviaList CreateSingleLineComment(string comment)
    {
        // Future: Here is how to write xml comment. https://stackoverflow.com/questions/30695752/how-do-i-add-an-xml-doc-comment-to-a-classdeclarationsyntax-in-roslyn
        return SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// "+ comment));
    }

    private static BaseNamespaceDeclarationSyntax GetNameSpace(TypeDeclarationSyntax syntax)
    {
        return syntax.Ancestors()
           .OfType<BaseNamespaceDeclarationSyntax>()
           .First();
    }

    /// <summary> Copied with pride from https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarch/
    /// determine the namespace the class/enum/struct is declared in, if any
    /// </summary>
    private static string GetNamespaceName(BaseTypeDeclarationSyntax syntax)
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

    private static IEnumerable<PropertyDeclarationSyntax> GetProperties(TypeDeclarationSyntax syntax)
    {
        return syntax.Members
            .Where(m => m is not null)
            .Cast<PropertyDeclarationSyntax>();
    }

    /// <summary>Get the flag UsePrivateConstructor from DtoAttribute.
    /// Ugly code; yes I know.
    /// </summary>
    private static bool GetUsePrivateConstructor(SyntaxList<AttributeListSyntax> attribList)
    {
        var attributeName = nameof(DtoAttribute); // DtoAttribute;
        var attributeNames = new[] { attributeName, attributeName.Replace("Attribute", "") };

        var usePrivateConstructorFieldName = nameof(DtoAttribute.UsePrivateConstructor);

        var attribute = attribList.SelectMany(als =>
                als.Attributes.Where(y => attributeNames.Contains(y.Name.ToString())))
            .Single();

        var usePrivateConstructorArgument =
            attribute.ArgumentList?.Arguments.FirstOrDefault(a =>
                a?.NameEquals?.Name.ToString() == usePrivateConstructorFieldName);

        if (usePrivateConstructorArgument is null)
        {
            return true;
        }

        // Get the argument value (i.e. the parameter) and try to get the value out of it.
        // If we cannot - return default value (true).
        var valueAsString = usePrivateConstructorArgument.Expression.NormalizeWhitespace().ToString();
        return bool.TryParse(valueAsString, out var usePrivateConstructorValue)
            ? usePrivateConstructorValue
            : true;
    }
}