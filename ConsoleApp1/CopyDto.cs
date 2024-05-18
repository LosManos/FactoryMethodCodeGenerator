using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record SourceType
{
    public int Value { get; init; }
    public string Name { get; init; }
}

[Dto]
public partial record TargetType
{
    public int Value { get; private set; }
    public string Name { get; init; }
}

[Map<SourceType, TargetType>(typeof(SourceType), typeof(TargetType))]
public abstract partial record Mapping
{
}