using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection
{
    public interface IServiceConfigurationProvider<TConfigurationKey, TConfiguration>
        where TConfiguration : ConfigurationBase<TConfigurationKey>
    {
        TConfiguration GetConfiguration(TConfigurationKey key);
    }
}