using Microsoft.CodeAnalysis;

namespace SourceGenerator;

internal static class DtoAttributeHelper
{
    private const string DtoAttributeMetadataName = "MyInterface.DtoAttribute";

    internal static INamedTypeSymbol GetDtoAttributeType(SemanticModel model)
    {
        return model.Compilation.GetTypeByMetadataName(DtoAttributeMetadataName)
            ?? throw new Exception($"Could not find {DtoAttributeMetadataName}.");
    }

    internal static bool HasGetDtoAttribute(
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        INamedTypeSymbol dtoAttributeType)
    {
        return GetDtoAttributeSymbolOrNull(attributeSymbols, dtoAttributeType) is not null;
    }

    private static INamedTypeSymbol? GetDtoAttributeSymbolOrNull(
        IEnumerable<INamedTypeSymbol> attributeSymbols,
        INamedTypeSymbol dtoAttributeType)
    {
        var validAttribute =
            attributeSymbols.FirstOrDefault(asym => asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }
}