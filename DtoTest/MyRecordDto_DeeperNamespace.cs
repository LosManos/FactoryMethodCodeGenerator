using MyInterface;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace DtoTest.Deeper.Namespace
{
    [Dto]
    public partial record MyRecordDto_With_DeeperNamespace
    {
        public int MyFirstValue { get; init; }
    }
}