using Microsoft.CodeAnalysis;

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
        var typeCollector = (TypeCollector)context.SyntaxReceiver!;
        var compilation = context.Compilation;
        // var model = compilation.GetSemanticModel(compilation.SyntaxTrees.First());

        var output = new List<string>();

        foreach (var classDeclaration in typeCollector.Types)
        {
            var has = classDeclaration.attribs.Any(a =>
                a.Name.ToString() == "Dto" || a.Name.ToString() == "DtoAttribute");

            var outputItem = $"{{{classDeclaration.isOfType}:{
                classDeclaration.type.GetDeclaredSymbol(compilation)},{
                    classDeclaration.type.GetType().Name},{
                        classDeclaration.attribs.Select(a => a.Name.ToString()).StringJoin()},{
                            has}}}";
            output.Add(outputItem);
        }

        // Build the source.
        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.Build(compilation, typeCollector.TypesWithAttribute("Dto", "DtoAttribute"));

        // Add the source codes to the compilation.
        foreach (var source in dtoSources)
        {
            context.AddSource($"MyDtos.{source.identifier}.g.cs", source.source);
        }
    }
}