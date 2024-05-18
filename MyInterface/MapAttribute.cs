namespace MyInterface;

/// <summary>Set this attribute on a class. Then all methods named `Map` will be mapping methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MapAttribute(
    Type source,
    Type target
) : Attribute
{
    public Type Source { get; } = source;
    public Type Target { get; } = target;
}