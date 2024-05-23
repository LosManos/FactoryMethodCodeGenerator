using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record CopySource
{
    public int Value { get; init; }
}

[Dto]
public partial record CopyTarget
{
    public int Value { get; private set; }
}

[Map<CopySource, CopyTarget>(typeof(CopySource), typeof(CopyTarget))]
public abstract partial record Mapping
{
//    public abstract CopyTarget Map<CopyTarget>(CopySource source);

// static partial void HelloFrom(string name);
}