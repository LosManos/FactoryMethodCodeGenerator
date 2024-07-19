using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal static class SyntaxHelper
{
    internal static BaseNamespaceDeclarationSyntax GetNameSpace(TypeDeclarationSyntax syntax)
    {
        return syntax.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .First();
    }

    internal static IEnumerable<PropertyDeclarationSyntax> GetProperties(TypeDeclarationSyntax syntax)
    {
        return syntax.Members
            .Where(m => m is not null)
            .Cast<PropertyDeclarationSyntax>();
    }
}