using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces;

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