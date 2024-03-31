using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

static class AttributeListSyntaxExtensions
{
    /// <summary>Copied with pride from
    /// https://stackoverflow.com/a/73868033/521554
    /// </summary>
    public static IReadOnlyList<AttributeData> GetAttributes(this AttributeListSyntax attributes,
        Compilation compilation)
    {
        // Collect pertinent syntax trees from these attributes
        var acceptedTrees = new HashSet<SyntaxTree>();
        foreach (var attribute in attributes.Attributes)
            acceptedTrees.Add(attribute.SyntaxTree);

        var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var ret = new List<AttributeData>();
        foreach (var attribute in parentAttributes)
        {
            if (acceptedTrees.Contains(attribute.ApplicationSyntaxReference!.SyntaxTree))
                ret.Add(attribute);
        }

        return ret;
    }
}