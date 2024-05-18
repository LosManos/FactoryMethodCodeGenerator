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
                // We would prefer to filter more thoroughly here, like on the correct attribute. But such a solution eludes me.
                predicate: (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);

        var recordSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                // We would prefer to filter more thoroughly here, like on the correct attribute. But such a solution eludes me.
                predicate: (node, _) => node is RecordDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) => (RecordDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);

        context.RegisterSourceOutput(classSyntaxProvider,
            static (spc, syntax) => ExecuteClass(spc, syntax));
        context.RegisterSourceOutput(recordSyntaxProvider,
            static (spc, syntax) => ExecuteRecord(spc, syntax));
    }

    static void ExecuteClass(SourceProductionContext spc, ClassDeclarationSyntax syntax)
    {
        // Bail early if we are not interested.
        if (syntax.TryGetDtoAttributeOrNull(out _) == false) return;

        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.BuildClass(spc, syntax);

        var sourceCode =
            "// " + DateTime.Now.ToString("u") + "\n" +
            dtoSources.source;
        var fileName = dtoSources.recordName + ".g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static void ExecuteRecord(SourceProductionContext spc, RecordDeclarationSyntax syntax)
    {
        if (syntax.TryGetDtoAttributeOrNull(out _) == false) return;

        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.BuildRecord(spc, syntax);

        var sourceCode =
            "// " + DateTime.Now.ToString("u") + "\n" +
            dtoSources.source;
        var fileName = dtoSources.recordName + ".g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }
}