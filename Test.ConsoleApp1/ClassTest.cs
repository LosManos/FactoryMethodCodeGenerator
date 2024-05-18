using ConsoleApp1;
using ConsoleApp1.Deeper.Namespace;
using FluentAssertions;

namespace Test.ConsoleApp1;

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
    }

    /// <summary>
    ///  A record/class without the attibute should not be manipulated.
    /// </summary>
    [Theory]
    [InlineData(typeof(NotAClassDto))]
    [InlineData(typeof(MyClassDto_With_DeeperNamespace))]
    public void Only_manipulate_WHEN_correct_tag(Type withoutDtoAttribute)
    {
        var createMethod = withoutDtoAttribute.GetMethods().FirstOrDefault(method => method.Name == "Create");
        createMethod.Should().BeNull();
    }
}
