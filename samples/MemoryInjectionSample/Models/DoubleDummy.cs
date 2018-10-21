using MemoryInjectionSample.Interfaces;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

namespace MemoryInjectionSample.Models
{
    [InjectionTarget("DoubleDummy")]
    public class DoubleDummy : IDummy
    {
        public string GetDummy()
        {
            return nameof(DoubleDummy);
        }
    }
}