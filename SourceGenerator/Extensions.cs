using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal static class Extensions
{
    internal static bool TryGetDtoAttribute(this TypeDeclarationSyntax syntax,
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        INamedTypeSymbol dtoAttributeType,
        out INamedTypeSymbol? result)
    {
        result = syntax.GetDtoAttributeSymbolOrNull(attributeSymbols, dtoAttributeType);
        return result is not null;
    }

    private static INamedTypeSymbol? GetDtoAttributeSymbolOrNull(this TypeDeclarationSyntax syntax,
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        INamedTypeSymbol dtoAttributeType)
    {
        var validAttribute =
            attributeSymbols.FirstOrDefault(asym => asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }
}