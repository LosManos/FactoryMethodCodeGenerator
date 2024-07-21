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

    public static bool HasMapAttribute(this SyntaxList<AttributeListSyntax> me)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        var hasMapAttribute = me
            .SelectMany(x => x.Attributes)
            .Any(a => (a.Name as GenericNameSyntax)?.Identifier.Text == "Map");
        return hasMapAttribute;
    }

    internal static IEnumerable<AttributeSyntax> GetMapAttributes(this IEnumerable<AttributeSyntax> attributes)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        return attributes
            .Where(a => (a.Name as GenericNameSyntax)?.Identifier.Text == "Map");
    }

    internal static IEnumerable<AttributeSyntax> GetMapAttributes(this SyntaxList<AttributeListSyntax> attributes)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        return GetMapAttributes(attributes.SelectMany(x => x.Attributes));
    }
}