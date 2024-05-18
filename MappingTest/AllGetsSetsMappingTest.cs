using MyInterface;

// ReSharper disable once CheckNamespace
namespace MappingTest.AllGetsSetsTypes;

[Dto]
public partial record SourceType
{
    public int IntValue { get; }
    public string StringValue { get; }
    public double DoubleValue { get; }
    public bool BooleanValue { get; }
}

[Dto]
public partial record TargetType
{
    public int IntValue { get; }
    public string StringValue { get; }
    public double DoubleValue { get; }
    public bool BooleanValue { get; }
}

[Map<SourceType, TargetType>(typeof(SourceType), typeof(TargetType))]
public abstract partial record Mapping
{
}

public class AllGetsSetsTypesMappingTest
{
    [Fact]
    public void MapsAll_gets_and_sets_variants()
    {
        var myIntValue = 42;
        var myStringValue = "fortytwo";
        var myDoubleValue = 42.42;
        var myBooleanValue = true;
        var source = SourceType.Create(myIntValue, myStringValue, myDoubleValue, myBooleanValue);

        //  Act.
        var target = Mapping.SourceType_To_TargetType(source);

        //  Assert.
        Assert.Equal(myStringValue, target.StringValue);
        Assert.Equal(myIntValue, target.IntValue);
        Assert.Equal(myDoubleValue, target.DoubleValue);
        Assert.Equal(myBooleanValue, target.BooleanValue);
    }
}