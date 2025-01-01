using MyInterface;

// ReSharper disable once CheckNamespace
namespace MappingTest.AttributeName;

[Dto]
public partial record SourceType
{
    public int Value { get; set; }
}

[Dto]
public partial record TargetType
{
    public int Value { get; set; }
}

[MapAttribute<SourceType, TargetType>(typeof(SourceType), typeof(TargetType))]
public abstract partial record Mapping
{
}

public class AttributeNameTest
{
    [Fact]
    public void MapsAllFields()
    {
        var myValue = 10;
        var source = SourceType.Create(myValue);

        //  Act.
        var target = Mapping.SourceType_To_TargetType(source);

        //  Assert.
        Assert.Equal(myValue, target.Value);
    }
}