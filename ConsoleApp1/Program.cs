namespace ConsoleApp1;

class Program
{
    static void Main(string[] args)
    {
        MyRecordDto_With_DifferentTypes.Create(42, "fortytwo", 4.2f);
        MyRecordDto_With_DefaultPrivateConstructor.Create(42);
        MyRecordDto_With_PublicPrivateConstructor.Create(42);
        new MyRecordDto_With_PublicPrivateConstructor(42);
        MyRecordDto_With_ExplicitPrivateConstructor.Create(42);

        MyClassDto.Create(42, "fortytwo", 4.2f);

        // var myRecord2 = MyRecordDto2.Create(42);
    }
}