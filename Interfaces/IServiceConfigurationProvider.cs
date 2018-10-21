using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection
{
    public interface IServiceConfigurationProvider<TConfigurationKey, TConfiguration>
    {
        TConfiguration GetConfiguration(TConfigurationKey key);
    }
}