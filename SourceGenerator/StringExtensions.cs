namespace SourceGenerator;

static class StringExtensions
{
    public static string StringJoin(this IEnumerable<string> me)
    {
        return $"[{string.Join(",", me)}]";
    }

    public static string StringJoinNL(this IEnumerable<string> me)
    {
        return $"[{string.Join(",\n ",me)}]".Replace("\n", "\n// ");
    }
}