using MyInterface;

// ReSharper disable once CheckNamespace
namespace MappingTest.AllTypes;

[Dto]
public partial record AllSourceType
{
    public int IntValue { get; }
    public string StringValue { get; }
    public double DoubleValue { get; }
    public bool BooleanValue { get; }
}

[Dto]
public partial record AllTargetType
{
    public int IntValue { get; }
    public string StringValue { get; }
    public double DoubleValue { get; }
    public bool BooleanValue { get; }
}

[Map<AllSourceType, AllTargetType>(typeof(AllSourceType), typeof(AllTargetType))]
public abstract partial record Mapping
{
}

public class AllTypesMappingTest
{
    [Fact]
    public void MapsAllFields()
    {
        var myIntValue = 42;
        var myStringValue = "fortytwo";
        var myDoubleValue = 42.42;
        var myBooleanValue = true;
        var source = AllSourceType.Create(myIntValue, myStringValue, myDoubleValue, myBooleanValue);

        //  Act.
        var target = Mapping.AllSourceType_To_AllTargetType(source);

        //  Assert.
        Assert.Equal(myStringValue, target.StringValue);
        Assert.Equal(myIntValue, target.IntValue);
        Assert.Equal(myDoubleValue, target.DoubleValue);
        Assert.Equal(myBooleanValue, target.BooleanValue);
    }
}