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

    internal static PropertyInfo Create(PropertyDeclarationSyntax? property)
    {
        return new PropertyInfo(
            property.Identifier.Text,
            property.Type,
            property.ToString());
    }
}

internal record ConstructorInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }

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

internal record RecordInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }

    private RecordInfo(string name, IEnumerable<PropertyInfo> properties)
    {
        Name = name;
        Properties = properties;
    }

    internal static RecordInfo Create(string name, IEnumerable<PropertyInfo> properties)
    {
        return new RecordInfo(name, properties);
    }
}

// internal record NamespaceInfo
// {
//     internal string Name { get; private set; }
//     internal IEnumerable<RecordInfo> Records { get; private set; }
//
//     private NamespaceInfo(string name, IEnumerable<RecordInfo> records)
//     {
//         Name = name;
//         Records = records;
//     }
//
//     internal static NamespaceInfo Create(string name, IEnumerable<RecordInfo> records)
//     {
//         return new NamespaceInfo(name, records);
//     }
// }