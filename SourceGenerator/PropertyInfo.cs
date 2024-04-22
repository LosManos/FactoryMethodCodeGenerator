using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal record PropertyInfo
{
    internal string Name { get; private set; }
    internal TypeSyntax Type { get; private set; }
    internal string Text { get; private set; }

    private PropertyInfo(string name, TypeSyntax type, string text)
    {
        Name = name;
        Type = type;
        Text = text;
    }

    // TODO:OF:Can we get rid of this?
    internal static PropertyInfo Create(string name, TypeSyntax type, string text)
    {
        return new PropertyInfo(name, type, text);
    }

    internal static PropertyInfo Create(PropertyDeclarationSyntax property)
    {
        return new PropertyInfo(
            property.Identifier.Text,
            property.Type,
            property.ToString());
    }
}