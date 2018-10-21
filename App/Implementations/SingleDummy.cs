using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Implementations
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