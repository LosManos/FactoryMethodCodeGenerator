namespace ConsoleApp1;

[Dto]
public record MyRecordDto(string name);

[Dto]
public class MyClassDto(string name);

public record NotARecordDto();

public class NotAClassDto();