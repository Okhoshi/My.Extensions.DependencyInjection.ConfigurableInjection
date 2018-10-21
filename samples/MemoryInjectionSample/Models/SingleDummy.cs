using MemoryInjectionSample.Interfaces;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

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