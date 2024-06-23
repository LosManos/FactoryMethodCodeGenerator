namespace MyInterface;

/// <summary>Set this attribute on a class. Then all methods named `Map` will be mapping methods.
/// By some, yet, unknown reason we need both the generic `TSource` and `TTarget`
/// and the arguments `source` and `target`. It would be nice if we could get rid of one of the pairs.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MapAttribute<TSource, TTarget>(
    Type source,
    Type target
) : Attribute
{
    public Type Source { get; } = source;
    public Type Target { get; } = target;
}