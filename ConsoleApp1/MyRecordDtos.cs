using MyInterface;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ConsoleApp1;

[Dto]
public partial record MyRecordDto_With_DifferentTypes
{
    public int MyFirstValue { get; init; }
    public string MySecondValue { get; init; }
    public float MyThirdValue { get; init; }
}

[Dto]
public partial record MyRecordDto_With_PrivateConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = false)]
public partial record MyRecordDto_With_PublicConstructor
{
    public int MyFirstValue { get; init; }
}

[Dto(UsePrivateConstructor = true)]
public partial record MyRecordDto_With_ExplicitPrivateConstructor
{
    public int MyFirstValue { get; init; }
}

public record NotARecordDto;

/// <summary>Should not get a factory method.
/// If it does - the code will not compile as the record is not partial.
/// Alas important it is not partial.
/// </summary>
[Serializable]
public record MyRecord_Without_DtoAttribute;
