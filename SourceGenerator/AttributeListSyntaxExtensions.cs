using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

static class AttributeListSyntaxExtensions
{
    private const string SimplifiedDtoAttributeName = "Dto";
    private const string SimplifiedMapAttributeName = "Map";

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


    #region DtoAttribute methods.

    /// <summary>Returns whether an attribute list has at least one DtoAttribute.
    /// The comparison is simplified in the way that it is _not_ for the fully qualified name.
    /// <remarks>
    /// There may be false positives if there is another attribute with the same name.
    /// </remarks>
    /// <remarks>
    /// Note that the DtoAttribute methods have different NameSyntax casting
    /// that MapAttribute methods.
    /// </remarks>
    /// </summary>
    /// <param name="me"></param>
    /// <returns></returns>
    internal static bool HasSimplifiedDtoAttribute(this SyntaxList<AttributeListSyntax> me)
    {
        // Get the Dto attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        var hasDtoAttribute = me
            .SelectMany(x => x.Attributes)
            .Any(a => (a.Name as IdentifierNameSyntax)?.Identifier.Text == SimplifiedDtoAttributeName);
        return hasDtoAttribute;
    }

    #endregion

    #region MapAttribute methods.

    /// <summary>Returns whether an attribute list has at least one MapAttribute.
    /// The comparison is simplified in the way that it is _not_ for the fully qualified name.
    /// <remarks>
    /// There may be false positives if there is another attribute with the same name.
    /// </remarks>
    /// <remarks>
    /// Note that the DtoAttribute methods have different NameSyntax casting
    /// that MapAttribute methods.
    /// </remarks>
    /// </summary>
    /// <param name="me"></param>
    /// <returns></returns>
    internal static bool HasSimplifiedMapAttribute(this SyntaxList<AttributeListSyntax> me)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        var hasMapAttribute = me
            .SelectMany(x => x.Attributes)
            .Any(a => (a.Name as GenericNameSyntax)?.Identifier.Text == SimplifiedMapAttributeName);
        return hasMapAttribute;
    }

    /// <summary>Gets all Map attributes for a list of attributes.
    /// The comparison is simplified in the way that it is _not_ for the fully qualified name.
    /// <remarks>
    /// There may be false positives if there is another attribute with the same name.
    /// </remarks>
    /// <remarks>
    /// Note that the DtoAttribute methods have different NameSyntax casting
    /// that MapAttribute methods.
    /// </remarks>
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    internal static IEnumerable<AttributeSyntax> GetSimplifiedMapAttributes(this IEnumerable<AttributeSyntax> attributes)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        return attributes
            .Where(a => (a.Name as GenericNameSyntax)?.Identifier.Text == SimplifiedMapAttributeName);
    }

    /// <summary>Gets all Map attributes for a list of attributes.
    /// The comparison is simplified in the way that it is _not_ for the fully qualified name.
    /// <remarks>
    /// There may be false positives if there is another attribute with the same name.
    /// </remarks>
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    internal static IEnumerable<AttributeSyntax> GetSimplifiedMapAttributes(this SyntaxList<AttributeListSyntax> attributes)
    {
        // Get the Map attribute.
        // We use a string to compare against and that is a recipe for disaster
        // as there can be many attributes with said name. Feel free to make a more clever solution.
        return GetSimplifiedMapAttributes(attributes.SelectMany(x => x.Attributes));
    }

    #endregion
}