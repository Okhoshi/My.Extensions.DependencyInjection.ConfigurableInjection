using MemoryInjectionSample.Interfaces;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

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