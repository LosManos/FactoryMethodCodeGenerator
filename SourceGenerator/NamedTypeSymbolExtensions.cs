using Microsoft.CodeAnalysis;

namespace SourceGenerator;

internal static class NamedTypeSymbolExtensions
{
    /// <summary>Returns if the item is a DtoAttribute somewhere.
    /// </summary>
    /// <param name="attributeSymbol"></param>
    /// <returns>True if the item is a DtoAttribute somewhere; false otherwise.</returns>
    internal static bool IsDtoAttribute(this INamedTypeSymbol attributeSymbol)
    {
        return
            attributeSymbol.OriginalDefinition.ContainingNamespace.Name == Constants.DtoAttributeNamespace &&
            attributeSymbol.Name == Constants.DtoAttributeMetadataName;
    }

    /// <summary>Returns if the item is a MapAttribute.
    /// </summary>
    /// <param name="attributeSymbol"></param>
    /// <returns>True if the item is a MapAttribute; false otherwise.</returns>
    internal static bool IsMapAttribute(this INamedTypeSymbol attributeSymbol)
    {
        return
            attributeSymbol.OriginalDefinition.ContainingNamespace.Name == Constants.MapAttributeNamespace &&
            attributeSymbol.Name == Constants.MapAttributeMetadataName;
    }
}