using MyInterface;

namespace DtoTestA
{
    /// <summary>/These classes are used for testing that two DTOs can have the same name.
    /// If it compiles, it works.
    /// </summary>
    [Dto]
    public partial class CommonClassName
    {
    }
}
namespace DtoTestB
{
    /// <summary>/These classes are used for testing that two DTOs can have the same name.
    /// If it compiles, it works.
    /// </summary>
    [Dto]
    public partial class CommonClassName
    {
    }
}
