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

        // Find the main method
        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken)
                         ?? throw new Exception("No main method found");

        // Build up the source code
        var sourceBuilder = new SourceCodeBuilder();

        var consoleSource = sourceBuilder.Build(output, mainMethod);

        // Add the source code to the compilation
        var typeName = mainMethod.ContainingType.Name;
        context.AddSource($"{typeName}.g.cs", consoleSource);

        var dtoSource = sourceBuilder.Build("MyGeneratedNamespace", output);
        // Add the source code to the compilation
        context.AddSource($"MyDtos.g.cs", dtoSource);
    }
}