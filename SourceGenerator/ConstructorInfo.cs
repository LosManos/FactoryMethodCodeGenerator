namespace SourceGenerator;

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