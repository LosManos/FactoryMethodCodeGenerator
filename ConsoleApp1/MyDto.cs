using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto
{
    public int MyFirstValue { get; set; }
    public string MySecondValue { get; set; }
    public float MyThirdValue { get; set; }
}

[Dto]
public partial class MyClassDto(string name);

public record NotARecordDto();

public class NotAClassDto();