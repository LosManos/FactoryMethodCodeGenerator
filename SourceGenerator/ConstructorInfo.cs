namespace SourceGenerator;

internal record ConstructorInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }
    public bool IsPrivateConstructor { get; }

    private ConstructorInfo(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        Name = name;
        Properties = properties;
        IsPrivateConstructor = isPrivateConstructor;
    }

    internal static ConstructorInfo Create(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        return new ConstructorInfo(name, properties, isPrivateConstructor);
    }
}