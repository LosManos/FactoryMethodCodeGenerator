namespace MappingTest.AnotherInterface;

/// <summary>An interface very alike the actual one. But we should be able to differ the two as they belong to different name spaces.
/// </summary>
/// <param name="sourceType"></param>
/// <param name="targetType"></param>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TTarget"></typeparam>
[AttributeUsage(AttributeTargets.Class)]
public class MapAttribute<TSource, TTarget>(
    Type sourceType,
    Type targetType
) : Attribute
{
    public Type SourceType { get; } = sourceType;
    public Type Target { get; } = targetType;
}