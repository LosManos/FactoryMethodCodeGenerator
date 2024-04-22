using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal record PropertyInfo
{
    internal string Name { get; private set; }
    internal TypeSyntax Type { get; private set; }

    private PropertyInfo(string name, TypeSyntax type)
    {
        Name = name;
        Type = type;
    }

    internal static PropertyInfo Create(string name, TypeSyntax type)
    {
        return new PropertyInfo(name, type);
    }

    internal static PropertyInfo Create(PropertyDeclarationSyntax? property)
    {
        return new PropertyInfo(
            property.Identifier.Text,
            property.Type);
    }
}

internal record ConstructorInfo
{
    public string Name { get; private set; }
    public IEnumerable<PropertyInfo> Properties { get; private set; }

    private ConstructorInfo(string name, IEnumerable<PropertyInfo> properties)
    {
        Name = name;
        Properties = properties;
    }

    internal static ConstructorInfo Create(string name, IEnumerable<PropertyInfo> properties)
    {
        return new ConstructorInfo(name, properties);
    }
}
