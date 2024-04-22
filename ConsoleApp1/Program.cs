namespace ConsoleApp1;

class Program
{
    static void Main(string[] args)
    {
        var myRecord1 = MyRecordDto1.Create(42, "fortytwo", 4.2f);
        var myRecord2 = MyRecordDto2.Create(42, "fortytwo", 4.2f);
        var myRecord3 = MyRecordDto3.Create(42, "fortytwo", 4.2f);
        var myClass = MyClassDto.Create(42, "fortytwo", 4.2f);
        // var myRecord2 = MyRecordDto2.Create(42);
    }
}