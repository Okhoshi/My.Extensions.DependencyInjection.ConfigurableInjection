using System;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Interfaces
{
    [InjectionTarget("Dummy")]
    public interface IDummy
    {
        string GetDummy();
    }
}