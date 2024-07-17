using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

/// <summary>See for origin of code:
/// https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/
/// </summary>
[Generator]
public class DtoIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                // We would prefer to filter more thoroughly here, like on the correct attribute. But such a solution eludes me. Maybe we can use the solution Map uses?
                predicate: (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);

        var recordSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                // We would prefer to filter more thoroughly here, like on the correct attribute. But such a solution eludes me. Maybe we can use the solution Map uses?
                predicate: (node, _) => node is RecordDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) => (RecordDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);

        var mapSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is RecordDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) =>
                {
                    var semanticModel = ctx.SemanticModel;
                    return (SemanticModel: semanticModel, Node: (RecordDeclarationSyntax)ctx.Node);
                })
            .Where(static rds => rds.Node is not null && rds.Node.AttributeLists.HasMapAttribute());

        context.RegisterSourceOutput(classSyntaxProvider,
            static (spc, syntax) => ExecuteDtoClass(spc, syntax));
        context.RegisterSourceOutput(recordSyntaxProvider,
            static (spc, syntax) => ExecuteDtoRecord(spc, syntax));
        context.RegisterSourceOutput(mapSyntaxProvider,
            static (spc, syntax) => ExecuteMapRecord((spc, syntax.SemanticModel), syntax.Node));
    }

    private static void ExecuteDtoClass(SourceProductionContext spc, ClassDeclarationSyntax syntax)
    {
        // Bail early if we are not interested.
        if (syntax.TryGetDtoAttributeOrNull(out _) == false) return;

        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.BuildDtoClass(spc, syntax);

        var sourceCode =
            "// Automagically built at: " + DateTime.UtcNow.ToString("u") + $" by {nameof(DtoIncrementalGenerator)}\n\n" +
            dtoSources.source;
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static void ExecuteDtoRecord(SourceProductionContext spc, RecordDeclarationSyntax syntax)
    {
        if (syntax.TryGetDtoAttributeOrNull(out _) == false) return;

        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.BuildDtoRecord(spc, syntax);

        var sourceCode =
            "// Automagically built at: " + DateTime.UtcNow.ToString("u") + $" by {nameof(DtoIncrementalGenerator)}\n\n" +
            dtoSources.source;
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static void ExecuteMapRecord((SourceProductionContext spc, SemanticModel semanticModel) context, RecordDeclarationSyntax syntax)
    {
        var dtoSources = SourceCodeBuilder.BuildMapRecord(context, syntax);

        var sourceCode =
            "// Automagically built at: " + DateTime.UtcNow.ToString("u") + $" by {nameof(DtoIncrementalGenerator)}\n\n" +
            dtoSources.source;
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        context.spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }
}