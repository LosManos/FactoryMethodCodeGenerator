using DtoTest.Deeper.Namespace;
using FluentAssertions;

namespace DtoTest;

public class ClassTest
{
    [Fact]
    public void Static_constructor_with_many_parameters()
    {
        _ = MyClassDto_With_DifferentTypes.Create(42, "fortytwo", 4.2f);
        // If it compiles it works.
    }

    [Fact]
    public void Private_constructor_without_attribute()
    {
        _ = MyClassDto_With_PrivateConstructor.Create(42);
        // If it compiles it works.
    }

    [Fact]
    public void Private_constructor_with_attribute()
    {
        _ = MyClassDto_With_ExplicitPrivateConstructor.Create(1);
        // If it compiles it works.
    }

    [Fact]
    public void Public_constructor()
    {
        _ = MyClassDto_With_PublicConstructor.Create(1);
        _ = new MyClassDto_With_PublicConstructor(1);
        // If it compiles it works.
    }

    [Fact]
    public void Deeper_namespace()
    {
        _ = MyClassDto_With_DeeperNamespace.Create(42);
        // If it compiles it works.
    }

    /// <summary>A record/class without the correct attribute should not be manipulated.
    /// </summary>
    [Theory]
    [InlineData(typeof(MyClassDto_With_DifferentTypes), true,
        "Succeeds in being a mapper class. For checking the test works.")]
    [InlineData(typeof(NotAClassDto), false,
        "No attribute at all should not create a mapping class.")]
    [InlineData(typeof(MyClassDto_With_Another_Namespace), false,
        "Wrong MapAttribute, e.g. coming from another dependency does not render a mapping class.")]
    public void Only_manipulate_WHEN_correct_tag(Type sut, bool expectedShouldBeManipulated, string reason)
    {
        // The static constructor "Create" should always be present for a valid Dto.
        const string methodName = "Create";
        var createMethod = sut.GetMethods().FirstOrDefault(method => method.Name == methodName);

        //  Assert.
        (createMethod is not null).Should().Be(expectedShouldBeManipulated, reason);
    }

    [Fact]
    public void AttributeMetaname()
    {
        _ = MyClassDto_With_Metaname.Create(1);
        // If it compiles it works.
    }
}
