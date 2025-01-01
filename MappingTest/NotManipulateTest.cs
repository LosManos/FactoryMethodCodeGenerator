using FluentAssertions;
using MyInterface;

// ReSharper disable InconsistentNaming
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once CheckNamespace

namespace MappingTest.NotManipulate;

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

public abstract partial record Mapping_Without_Attribute
{
}

[AnotherInterface.MapAttribute<SourceType, TargetType>(typeof(SourceType), typeof(TargetType))]
public abstract partial record Mapping_With_Wrong_Attribute
{
}

public class AttributeNameTest
{
    /// <summary>A record/class without the correct attribute should not be manipulated.
    /// </summary>
    [Theory]
    [InlineData(typeof(Mapping), true,
        "Succeeds in being a mapper class. For checking the test works.")]
    [InlineData(typeof(Mapping_Without_Attribute), false,
        "No attribute at all should not create a mapping class.")]
    [InlineData(typeof(Mapping_With_Wrong_Attribute), false,
        "Wrong MapAttribute, e.g. coming from another dependency does not render a mapping class.")]
    public void MapsAllFields(Type sut, bool expectedShouldBeManipulated, string reason)
    {
        // The method "SourceType_To_TargetType" should always be present for a valid Dto.
        const string methodName = "SourceType_To_TargetType";
        var createMethod = sut.GetMethods().FirstOrDefault(method => method.Name == methodName);

        //  Assert.
        (createMethod is not null).Should().Be(expectedShouldBeManipulated, reason);
    }
}