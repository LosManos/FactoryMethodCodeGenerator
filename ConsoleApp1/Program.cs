namespace ConsoleApp1;

class Program
{
    static void Main()
    {
    }
}

public abstract partial record Mapping
{
    void RenameMe()
    {
        var _ = Mapping.CopySource_To_CopyTarget(default);
    }
}