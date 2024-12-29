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
}