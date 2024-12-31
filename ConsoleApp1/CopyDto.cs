using MyInterface;

namespace ConsoleApp1;

/// <summary>We this attribute `DtoAttribute`, and not the normal `Dto` as to catch both kinds when debugging.
/// It is easier than catching flow in the source generation.
/// </summary>
[DtoAttribute]
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