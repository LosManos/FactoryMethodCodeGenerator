using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

static class Extensions
{
    public static IReadOnlyList<AttributeSyntax> GetAttributes(this TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists.SelectMany(al => al.Attributes).ToList();
    }

    public static AttributeSyntax? GetDtoAttributeOrNull(this TypeDeclarationSyntax syntax)
    {
        return syntax.GetAttributes().FirstOrDefault(a => a.Name.ToString() == "Dto");
    }

    public static bool TryGetDtoAttributeOrNull(this TypeDeclarationSyntax syntax, out AttributeSyntax? result)
    {
        // Get the Dto attribute.
        // We use the string Dto to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        var res = syntax.GetDtoAttributeOrNull();
        if (res is null)
        {
            // We cannot use [NotNullWhen(b)] as we are not running DotnetStandard 2.1.
            // Alas we hae to set the `result`.
            result = null;
            return false;
        }
        else
        {
            result = res;
            return true;
        }
    }
}