using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyInterface;

namespace SourceGenerator;

internal partial class SourceCodeBuilder
{
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

    private static SyntaxTriviaList CreateSingleLineComment(string comment)
    {
        // Future: Here is how to write xml comment. https://stackoverflow.com/questions/30695752/how-do-i-add-an-xml-doc-comment-to-a-classdeclarationsyntax-in-roslyn
        return SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// "+ comment));
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
        return !bool.TryParse(valueAsString, out var usePrivateConstructorValue) || usePrivateConstructorValue;
    }
}