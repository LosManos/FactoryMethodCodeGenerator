using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal static class Extensions
{
    internal static bool TryGetDtoAttribute(this TypeDeclarationSyntax syntax,
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        IEnumerable<AttributeSyntax> attributes,
        INamedTypeSymbol dtoAttributeType,
        out INamedTypeSymbol? result)
    {
        var res = syntax.GetDtoAttributeSymbolOrNull(attributeSymbols, attributes, dtoAttributeType);
        if (res is null)
        {
            // We cannot use [NotNullWhen(b)] as we are not running DotnetStandard 2.1.
            // Alas, we have to set the `result`.
            result = null;
            return false;
        }
        else
        {
            result = res;
            return true;
        }
    }

    [Obsolete("Prefer TryGetDtoAttreibute", false)]
    internal static bool TryGetDtoAttributeOrNull(this TypeDeclarationSyntax syntax, out AttributeSyntax? result)
    {
        var res = syntax.GetDtoAttributeOrNull();
        if (res is null)
        {
            // We cannot use [NotNullWhen(b)] as we are not running DotnetStandard 2.1.
            // Alas, we have to set the `result`.
            result = null;
            return false;
        }
        else
        {
            result = res;
            return true;
        }
    }

    private static IReadOnlyList<AttributeSyntax> GetAttributes(this TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists.SelectMany(al => al.Attributes).ToList();
    }

    /// <summary>Get the Dto attribute.
    /// We use the string Dto to compare against and that is a recipe for disaster
    /// as there can be many attributes with said name. Feel free to make a more clever solution.
    /// </summary>
    /// <param name="syntax"></param>
    /// <returns></returns>
    private static AttributeSyntax? GetDtoAttributeOrNull(this TypeDeclarationSyntax syntax)
    {
        // TODO:OF:Do a better comparison.
        return syntax.GetAttributes().FirstOrDefault(a => a.Name.ToString() == "Dto");
    }

    private static INamedTypeSymbol? GetDtoAttributeSymbolOrNull(this TypeDeclarationSyntax syntax,
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        IEnumerable<AttributeSyntax> attributes,
        INamedTypeSymbol dtoAttributeType)
    {
        var validAttribute =
            attributeSymbols.FirstOrDefault(asym => asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }
}