namespace MemoryInjectionSample.Models
{
    [InjectionTarget("SingleDunny")]
    public class SingleDunny : IDunny
    {
        public string GetDunny()
        {
            return nameof(SingleDunny);
        }
    }
}