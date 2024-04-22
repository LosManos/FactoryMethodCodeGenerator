using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto_With_DifferentTypes
{
    public int MyFirstValue { get; init; }
    public string MySecondValue { get; init; }
    public float MyThirdValue { get; init; }
}

[Dto]
public partial record MyRecordDto_With_DefaultPrivateConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = false)]
public partial record MyRecordDto_With_PublicPrivateConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = true)]
public partial record MyRecordDto_With_ExplicitPrivateConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto]
public partial class MyClassDto
{
    public int MyFirstValue { get; init; }
    public string MySecondValue { get; init; }
    public float MyThirdValue { get; init; }
}

public record NotARecordDto();

public class NotAClassDto();