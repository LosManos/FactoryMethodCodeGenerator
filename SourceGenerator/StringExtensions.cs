namespace SourceGenerator;

static class StringExtensions
{
    public static string StringJoin(this IEnumerable<string> me)
    {
        return $"[{string.Join(",", me)}]";
    }
}