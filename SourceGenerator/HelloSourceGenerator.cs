using Microsoft.CodeAnalysis;
using MyInterface;

namespace SourceGenerator;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new TypeCollector());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var attributeName = nameof(DtoAttribute); // DtoAttribute;
        var attributeNames = new[] { attributeName, attributeName.Replace("Attribute", "") };

        var typeCollector = (TypeCollector)context.SyntaxReceiver!;
        var compilation = context.Compilation;

        // Build the source.
        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.Build(compilation, typeCollector.TypesWithAttribute(attributeNames));

        // Add the source codes to the compilation.
        foreach (var source in dtoSources)
        {
            context.AddSource($"{source.namespaceName}.{source.classOrRecordName}.g.cs", source.source);
        }
    }
}