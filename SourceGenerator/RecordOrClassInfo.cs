namespace SourceGenerator;

internal abstract record RecordOrClassInfo
{
    internal string Name { get; private set; }
    internal IEnumerable<PropertyInfo> Properties { get; private set; }
    internal bool IsPrivateConstructor { get; private set; }

    private protected RecordOrClassInfo(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        Name = name;
        Properties = properties;
        IsPrivateConstructor = isPrivateConstructor;
    }
}

internal record ClassInfo : RecordOrClassInfo {
    private ClassInfo(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
        : base(name, properties, isPrivateConstructor)
    {
    }

    internal static ClassInfo Create(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        return new ClassInfo(name, properties, isPrivateConstructor);
    }
}

internal record RecordInfo : RecordOrClassInfo {
    private RecordInfo(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
        : base(name, properties, isPrivateConstructor)
    {
    }

    internal static RecordInfo Create(string name, IEnumerable<PropertyInfo> properties, bool isPrivateConstructor)
    {
        return new RecordInfo(name, properties, isPrivateConstructor);
    }
}