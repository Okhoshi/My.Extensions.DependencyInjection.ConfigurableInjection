using MemoryInjectionSample.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using static MemoryInjectionSample.Models.MemoryServiceConfigurationProvider;

namespace MemoryInjectionSample.Models
{
    [InjectionTarget("DoubleDummy")]
    public class DoubleDummy : IDummy
    {
        private readonly Counter counter;

        public DoubleDummy(IOptions<Counter> l)
        {
            counter = l.Value;
        }

        public string GetDummy()
        {
            return nameof(DoubleDummy) + $" {counter.Count}";
        }
    }

    public class Counter
    {
        public int Count { get; set; }
    }
}