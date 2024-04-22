namespace SourceGenerator;

internal record RecordOrClassInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }
    internal bool IsPrivateConstructor { get; private set; }

    private RecordOrClassInfo(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        Name = name;
        Properties = properties;
        IsPrivateConstructor = isPrivateConstructor;
    }

    internal static RecordOrClassInfo Create(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        return new RecordOrClassInfo(name, properties, isPrivateConstructor);
    }
}