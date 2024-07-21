using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal static class SyntaxHelper
{
    internal static IEnumerable<AttributeSyntax> GetAttributes(
        RecordDeclarationSyntax syntax,
        Func<SyntaxList<AttributeListSyntax>, IEnumerable<AttributeSyntax>> filter)
    {
        return filter(syntax.AttributeLists);
    }

    internal static string GetNameSpaceName(TypeDeclarationSyntax syntax)
    {
        return GetNameSpace(syntax).Name.ToString();
    }

    internal static IEnumerable<PropertyDeclarationSyntax> GetProperties(TypeDeclarationSyntax syntax)
    {
        return syntax.Members
            .Where(m => m is not null)
            .Cast<PropertyDeclarationSyntax>();
    }

    internal static string GetRecordNameString(RecordDeclarationSyntax syntax)
    {
        return syntax.Identifier.Text
            ?? throw new Exception("Source generation error. The syntax identifier does not have a name."); // This should not happen IRL.
    }

    private static BaseNamespaceDeclarationSyntax GetNameSpace(TypeDeclarationSyntax syntax)
    {
        return syntax.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .First();
    }
}