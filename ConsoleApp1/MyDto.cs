using MyInterface;

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto(string name);

[Dto]
public partial class MyClassDto(string name);

public record NotARecordDto();

public class NotAClassDto();