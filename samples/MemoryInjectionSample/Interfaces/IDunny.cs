using System;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces
{
    [InjectionTarget("Dunny")]
    public interface IDunny
    {
        string GetDunny();
    }
}