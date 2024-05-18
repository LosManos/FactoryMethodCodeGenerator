using MyInterface;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ConsoleApp1;

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

/// <summary>Should not get a factory method.
/// If it does - the code will not compile as the class is not partial.
/// Alas important it is not partial.
/// </summary>
[Serializable]
public class MyClass_Without_DtoAttribute;
