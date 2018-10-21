namespace MemoryInjectionSample.Models
{
    [InjectionTarget("SingleDummy")]
    public class SingleDummy : IDummy
    {
        public string GetDummy()
        {
            return nameof(SingleDummy);
        }
    }
}