using Microsoft.CodeAnalysis;

namespace SourceGenerator;

/// <summary>This class contains helper functions for the DtoAttribute.
/// </summary>
internal static class DtoAttributeHelper
{
    internal static bool HasGetDtoAttribute(
        SemanticModel model,
        IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        return GetDtoAttributeSymbolOrNull(model, attributeSymbols) is not null;
    }

    private static INamedTypeSymbol? GetDtoAttributeSymbolOrNull(
        SemanticModel model,
        IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        var dtoAttributeType = model.GetDtoAttributeType();

        var validAttribute =
            attributeSymbols.FirstOrDefault(asym =>
                asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }
}