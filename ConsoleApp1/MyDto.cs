using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto
{
    public string MyValue { get; set; }
}

[Dto]
public partial class MyClassDto(string name);

public record NotARecordDto();

public class NotAClassDto();