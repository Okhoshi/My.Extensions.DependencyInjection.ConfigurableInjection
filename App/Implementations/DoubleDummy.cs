using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Implementations
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