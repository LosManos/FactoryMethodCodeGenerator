using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record CopySource
{
    public int Value { get; init; }
    public string Name { get; init; }
}

[Dto]
public partial record CopyTarget
{
    public int Value { get; private set; }
    public string Name { get; init; }
}

[Map<CopySource, CopyTarget>(typeof(CopySource), typeof(CopyTarget))]
public abstract partial record Mapping
{
//    public abstract CopyTarget Map<CopyTarget>(CopySource source);

// static partial void HelloFrom(string name);
}