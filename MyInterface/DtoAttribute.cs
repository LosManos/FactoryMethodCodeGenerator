namespace MyInterface;

[AttributeUsage(AttributeTargets.Class)]
public class DtoAttribute : Attribute
{
    public bool UsePrivateConstructor { get; set; } = true;

    internal static readonly string UsePrivateConstructorFieldName = nameof(UsePrivateConstructor);
}