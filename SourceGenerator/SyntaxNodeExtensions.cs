using Microsoft.CodeAnalysis;

namespace SourceGenerator;

static class SyntaxNodeExtensions
{
    /// <summary>Copied with pride from
    /// https://stackoverflow.com/a/73868033/521554
    /// </summary>
    public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        return model.GetDeclaredSymbol(node);
    }
}