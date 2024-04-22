namespace SourceGenerator;

internal record RecordOrClassInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }

    private RecordOrClassInfo(string name, IEnumerable<PropertyInfo> properties)
    {
        Name = name;
        Properties = properties;
    }

    internal static RecordOrClassInfo Create(string name, IEnumerable<PropertyInfo> properties)
    {
        return new RecordOrClassInfo(name, properties);
    }
}