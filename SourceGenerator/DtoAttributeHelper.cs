using Microsoft.CodeAnalysis;

namespace SourceGenerator;

/// <summary>This class contains helper functions for the DtoAttribute.
/// </summary>
internal static class DtoAttributeHelper
{
    private const string DtoAttributeMetadataName = "MyInterface.DtoAttribute";

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
        var dtoAttributeType = GetDtoAttributeType(model);

        var validAttribute =
            attributeSymbols.FirstOrDefault(asym =>
                asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }

    private static INamedTypeSymbol GetDtoAttributeType(SemanticModel model)
    {
        return model.Compilation.GetTypeByMetadataName(DtoAttributeMetadataName)
               ?? throw new Exception($"Could not find {DtoAttributeMetadataName}.");
    }
}