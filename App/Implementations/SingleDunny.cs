using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Implementations
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