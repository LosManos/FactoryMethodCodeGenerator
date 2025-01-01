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
                predicate: (node, _) =>
                    node is ClassDeclarationSyntax { AttributeLists.Count: >= 1 }
                    && ((ClassDeclarationSyntax)node).AttributeLists.HasSimplifiedDtoAttribute(),
                transform: (ctx, _) => (SemanticModel: ctx.SemanticModel, Node: (ClassDeclarationSyntax)ctx.Node))
            // In the CreateSyntaxProvider we only have a Node, but we also need the model to figure out meta name and namespace. Hence, we have to add a where-filter.
            .Where(static clsInfo =>
                GetAttributeSymbols(clsInfo.SemanticModel, clsInfo.Node).Any(x => x.IsDtoAttribute())
            );

        var recordSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) =>
                    node is RecordDeclarationSyntax { AttributeLists.Count: >= 1 }
                    && ((RecordDeclarationSyntax)node).AttributeLists.HasSimplifiedDtoAttribute(),
                transform: (ctx, _) => (SemanticModel: ctx.SemanticModel, Node: (RecordDeclarationSyntax)ctx.Node))
            // In the CreateSyntaxProvider we only have a Node, but we also need the model to figure out meta name and namespace. Hence, we have to add a where-filter.
            .Where(static recInfo =>
                GetAttributeSymbols(recInfo.SemanticModel, recInfo.Node).Any(x => x.IsDtoAttribute())
            );

        var mapSyntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) =>
                    node is RecordDeclarationSyntax { AttributeLists.Count: >= 1 }
                    && ((RecordDeclarationSyntax)node).AttributeLists.HasSimplifiedMapAttribute(),
                transform: (ctx, _) => (SemanticModel: ctx.SemanticModel, Node: (RecordDeclarationSyntax)ctx.Node))
            // In the CreateSyntaxProvider we only have a Node, but we also need the model to figure out meta name and namespace. Hence, we have to add a where-filter.
            .Where(static recInfo =>
                GetAttributeSymbols(recInfo.SemanticModel, recInfo.Node).Any(x => x.IsMapAttribute())
            );

        context.RegisterSourceOutput(classSyntaxProvider,
            static (spc, syntax) => ExecuteDtoClass(spc, syntax.SemanticModel, syntax.Node));
        context.RegisterSourceOutput(recordSyntaxProvider,
            static (spc, syntax) => ExecuteDtoRecord(spc, syntax.SemanticModel, syntax.Node));
        context.RegisterSourceOutput(mapSyntaxProvider,
            static (spc, syntax) => ExecuteMapRecord((spc, syntax.SemanticModel), syntax.Node));
    }

    private static void ExecuteDtoClass(SourceProductionContext spc, SemanticModel _, ClassDeclarationSyntax syntax)
    {
        var dtoSources = SourceCodeBuilderDto.BuildDtoClass(spc, syntax);

        var sourceCode = CreateSourceCode(dtoSources);
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static void ExecuteDtoRecord(SourceProductionContext spc,SemanticModel model, RecordDeclarationSyntax syntax)
    {
        var dtoSources = SourceCodeBuilderDto.BuildDtoRecord(spc, syntax);

        var sourceCode = CreateSourceCode(dtoSources);
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static void ExecuteMapRecord((SourceProductionContext spc, SemanticModel semanticModel) context, RecordDeclarationSyntax syntax)
    {
        var dtoSources = SourceCodeBuilderMap.BuildMapRecord(context, syntax);

        var sourceCode = CreateSourceCode(dtoSources);
        var fileName = $"{dtoSources.namespaceName}.{dtoSources.recordName}.g.cs";
        context.spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static IEnumerable<INamedTypeSymbol> GetAttributeSymbols(
        SemanticModel model,
        TypeDeclarationSyntax syntax)
    {
        var attributes = syntax.AttributeLists
            .SelectMany(attrList => attrList.Attributes);

        var attributeSymbols = attributes
            .Select(asx =>
                model.GetSymbolInfo(asx).Symbol?.ContainingType)
            .Where(x => x != null)
            .Select(x => x!);
        return attributeSymbols;
    }

    private static string CreateSourceCode((string source, string namespaceName, string recordName) dtoSources)
    {
        return "// Automagically built at: " + DateTime.UtcNow.ToString("u") + $" by {nameof(DtoIncrementalGenerator)}\n\n" +
               dtoSources.source;
    }
}