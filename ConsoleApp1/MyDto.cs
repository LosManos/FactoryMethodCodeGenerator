using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto
{
    public int MyFirstValue { get; init; }
    public string MySecondValue { get; init; }
    public float MyThirdValue { get; init; }
}

public partial record MyRecordThatisNotIndlucedDto
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