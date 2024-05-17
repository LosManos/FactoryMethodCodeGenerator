using ConsoleApp1;

namespace Test.ConsoleApp1;

public class RecordTest
{
    [Fact]
    public void Static_constructor_with_many_parameters()
    {
        _ = MyRecordDto_With_DifferentTypes.Create(42, "fortytwo", 4.2f);
        // If it compiles it works.
    }

    [Fact]
    public void Private_constructor_without_attribute()
    {
        _ = MyRecordDto_With_PrivateConstructor.Create(42);
        // If it compiles it works.
    }

    [Fact]
    public void Private_constructor_with_attribute()
    {
        _ = MyRecordDto_With_ExplicitPrivateConstructor.Create(1);
        // If it compiles it works.
    }

    [Fact]
    public void Public_constructor()
    {
        _ = MyRecordDto_With_PublicConstructor.Create(1);
        _ = new MyRecordDto_With_PublicConstructor(1);
        // If it compiles it works.
    }
}
