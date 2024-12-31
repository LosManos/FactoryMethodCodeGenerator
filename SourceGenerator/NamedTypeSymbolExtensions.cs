using Microsoft.CodeAnalysis;

namespace SourceGenerator;

internal static class NamedTypeSymbolExtensions
{
    private const string DtoAttributeNamespace = "MyInterface";
    private const string DtoAttributeMetadataName = "DtoAttribute";

    private const string MapAttributeNamespace = "MyInterface";
    private const string MapAttributeMetadataName = "MapAttribute";

    /// <summary>Returns if the item is a DtoAttribute somewhere.
    /// </summary>
    /// <param name="attributeSymbol"></param>
    /// <returns>True if the item is a DtoAttribute somewhere; false otherwise.</returns>
    internal static bool IsDtoAttribute(this INamedTypeSymbol attributeSymbol)
    {
        return
            attributeSymbol.OriginalDefinition.ContainingNamespace.Name == DtoAttributeNamespace &&
            attributeSymbol.Name == DtoAttributeMetadataName;
    }

    /// <summary>Returns if the item is a MapAttribute.
    /// </summary>
    /// <param name="attributeSymbol"></param>
    /// <returns>True if the item is a MapAttribute; false otherwise.</returns>
    internal static bool IsMapAttribute(this INamedTypeSymbol attributeSymbol)
    {
        return
            attributeSymbol.OriginalDefinition.ContainingNamespace.Name == MapAttributeNamespace &&
            attributeSymbol.Name == MapAttributeMetadataName;
    }
}