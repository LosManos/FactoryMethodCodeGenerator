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

    /// <summary>Returns the DtoAttribute type.
    /// If not found it throws an exception.
    /// </summary>
    /// <param name="model">The model to search in.</param>
    /// <returns>The type of the DtoAttribute</returns>
    /// <exception cref="Exception">Thrown if the type is not found.</exception>
    internal static INamedTypeSymbol GetDtoAttributeType(this SemanticModel model)
    {
        return model.Compilation.GetTypeByMetadataName(DtoAttributeMetadataName)
               ?? throw new Exception($"Could not find {DtoAttributeMetadataName}.");
    }
}