using Microsoft.CodeAnalysis;

namespace SourceGenerator;

internal static class ModelExtensions
{
    /// <summary>The fully qualified name of the DtoAttribute.
    /// <remarks>
    /// It looks like code like `typeof(MyInterface.DtoAttribute)`
    /// should be usable to get to the name. But then we already have the type...
    /// Why we must use a magic string here is that the type is not yet loaded
    /// and `typeof(MyInterface.DtoAttribute)` throws an exception.
    /// </remarks>
    /// </summary>
    private const string DtoAttributeMetadataName = "MyInterface.DtoAttribute";

    private const string MapAttributeNamespace = "MyInterface";
    private const string MapAttributeMetadataName = "MapAttribute";

    /// <summary>Returns whether the model contains the DtoAttribute somewhere.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="attributeSymbols"></param>
    /// <returns>True if the model contains the DtoAttribute somewhere; false otherwise.</returns>
    internal static bool HasDtoAttribute(
        this SemanticModel model,
        IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        var dtoAttributeType = model.GetDtoAttributeType();
        return GetDtoAttributeSymbolOrNull(dtoAttributeType, attributeSymbols) is not null;
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

    /// <summary>Returns the DtoAttribute type.
    /// If not found it throws an exception.
    /// </summary>
    /// <param name="model">The model to search in.</param>
    /// <returns>The type of the DtoAttribute</returns>
    /// <exception cref="Exception">Thrown if the type is not found.</exception>
    private static INamedTypeSymbol GetDtoAttributeType(this SemanticModel model)
    {
        return model.Compilation.GetTypeByMetadataName(DtoAttributeMetadataName)
               ?? throw new Exception($"Could not find {DtoAttributeMetadataName}.");
    }

    private static INamedTypeSymbol? GetDtoAttributeSymbolOrNull(
        INamedTypeSymbol dtoAttributeType,
        IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        var validAttribute =
            attributeSymbols.FirstOrDefault(asym =>
                asym.ToDisplayString() == dtoAttributeType.ToDisplayString());
        return validAttribute;
    }
}