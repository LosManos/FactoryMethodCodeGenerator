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