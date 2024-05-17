using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

// https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/

[Generator]
public class DtoIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Instead of RegisterForSyntaxNotifications, you use Providers
        var classSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: >= 1 },
                transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static m => m is not null);
        var recordSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
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
        var sourceBuilder = new SourceCodeBuilder();
        var dtoSources = sourceBuilder.BuildRecord(spc, syntax);

        var sourceCode =
            "// " + DateTime.Now.ToString("u") + "\n" +
            dtoSources.source;
        var fileName = dtoSources.recordName + ".g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }
}