namespace ConsoleApp1;

class Program
{
    static void Main(string[] args)
    {
        var myRecord = MyRecordDto.Create(42, "fortytwo", 4.2f);
        var myClass = MyClassDto.Create(42, "fortytwo", 4.2f);
    }
}