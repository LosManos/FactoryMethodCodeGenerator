using MyInterface;

// ReSharper disable once CheckNamespace
namespace MappingTest.Simple;

[Dto]
public partial record SourceType
{
    public int Value { get; set; }
    public string Name { get; set; } = "";
}

[Dto]
public partial record TargetType
{
    public int Value { get; set; }
    public string Name { get; set; } = "";
}

[Map<SourceType, TargetType>(typeof(SourceType), typeof(TargetType))]
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
        var source = SourceType.Create(myValue, myName);

        //  Act.
        var target = Mapping.SourceType_To_TargetType(source);

        //  Assert.
        Assert.Equal(myValue, target.Value);
        Assert.Equal(myName, target.Name);
    }
}