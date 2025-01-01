namespace DtoTest.AnotherInterface;

/// <summary>An interface very alike the actual one. But we should be able to differ the two as they belong to different name spaces.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DtoAttribute : Attribute
{
    public bool UsePrivateConstructor { get; set; } = true;

#pragma warning disable CS0414 // Field is assigned but its value is never used
    internal static readonly string UsePrivateConstructorFieldName = nameof(UsePrivateConstructor);
#pragma warning restore CS0414 // Field is assigned but its value is never used
}