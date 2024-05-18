using MyInterface;

// ReSharper disable once CheckNamespace
namespace MappingTest.Simple;

[Dto]
public partial record SimpleSourceType
{
    public int Value { get; set; }
    public string Name { get; set; } = "";
}

[Dto]
public partial record SimpleTargetType
{
    public int Value { get; set; }
    public string Name { get; set; } = "";
}

[Map<SimpleSourceType, SimpleTargetType>(typeof(SimpleSourceType), typeof(SimpleTargetType))]
public abstract partial record Mapping
{
}

public class SimpleTypeMappingTest
{
    [Fact]
    public void MapsAllFields()
    {
        var myValue = 10;
        var myName = "Test";
        var source = SimpleSourceType.Create(myValue, myName);

        //  Act.
        var target = Mapping.SimpleSourceType_To_SimpleTargetType(source);

        //  Assert.
        Assert.Equal(myValue, target.Value);
        Assert.Equal(myName, target.Name);
    }
}