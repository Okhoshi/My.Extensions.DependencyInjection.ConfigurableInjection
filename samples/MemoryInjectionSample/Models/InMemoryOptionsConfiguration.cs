using MemoryInjectionSample.Models;
using Microsoft.Extensions.Options;

namespace MemoryInjectionSample
{
    public class InMemoryOptionsConfiguration : IConfigureOptions<Counter>
    {
        private int counter;

        public void Configure(Counter options)
        {
            options.Count = counter++;
        }
    }
}