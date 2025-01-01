using MyInterface;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DtoTest;

[Dto]
public partial class MyClassDto_With_DifferentTypes
{
    public int MyFirstValue { get; init; }
    public string MySecondValue { get; init; }
    public float MyThirdValue { get; init; }
}

[Dto]
public partial class MyClassDto_With_PrivateConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = false)]
public partial class MyClassDto_With_PublicConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = true)]
public partial class MyClassDto_With_ExplicitPrivateConstructor
{
    public int MyFirstValue { get; init; }
}

public class NotAClassDto;

/// <summary>This class should keep its Attribute suffix as it is the naming we test.
/// </summary>
// ReSharper disable once RedundantAttributeSuffix
[DtoAttribute]
public partial class MyClassDto_With_Metaname
{
    public int MyFirstValue { get; init; }
}

/// <summary>Should not get a factory method.
/// If it does - the code will not compile as the class is not partial.
/// Alas important it is not partial.
/// </summary>
[Serializable]
public class MyClass_Without_DtoAttribute;

/// <summary>This class/record shows every sign of being a DTO but does not refer to the correct attribute.
/// </summary>
[AnotherInterface.Dto]
public partial class MyClassDto_With_Another_Namespace
{
    /// <summary>We have a property, albeit unused, because we want to look like a real DTO in every aspect.
    /// </summary>
    public int AnyValue { get; init; }
}
