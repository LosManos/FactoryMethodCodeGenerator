using Microsoft.CodeAnalysis;

namespace SourceGenerator;

internal static class ModelExtensions
{
    private const string DtoAttributeNamespace = "MyInterface";
    private const string DtoAttributeMetadataName = "DtoAttribute";

    private const string MapAttributeNamespace = "MyInterface";
    private const string MapAttributeMetadataName = "MapAttribute";

    /// <summary>Returns whether the model contains the DtoAttribute somewhere.
    /// </summary>
    /// <param name="attributeSymbols"></param>
    /// <returns>True if the model contains the DtoAttribute somewhere; false otherwise.</returns>
    internal static bool HasDtoAttribute(IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        return attributeSymbols.Any(x =>
            x.OriginalDefinition.ContainingNamespace.Name == DtoAttributeNamespace &&
            x.Name == DtoAttributeMetadataName
        );
    }

    /// <summary>Returns whether the list contains the MapAttribute.
    /// </summary>
    /// <param name="attributeSymbols"></param>
    /// <returns>True if the list contains the MapAttribute; false otherwise.</returns>
    internal static bool HasMapAttribute(
        IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        return attributeSymbols.Any(x =>
            x.OriginalDefinition.ContainingNamespace.Name == MapAttributeNamespace &&
            x.Name == MapAttributeMetadataName
        );
    }
}