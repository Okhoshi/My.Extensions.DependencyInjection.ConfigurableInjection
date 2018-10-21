using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Implementations
{
    public abstract class MultiServiceConfigurationProviderBase<TKey, TConfigurationKey, TConfiguration> : IServiceConfigurationProvider<TConfigurationKey, TConfiguration>
        where TConfiguration : ConfigurationBase<TConfigurationKey>
    {
        private ConcurrentDictionary<TKey, Dictionary<TConfigurationKey, TConfiguration>> _ServiceConfigurations;

        public MultiServiceConfigurationProviderBase()
        {
            _ServiceConfigurations = new ConcurrentDictionary<TKey, Dictionary<TConfigurationKey, TConfiguration>>();
        }

        public TConfiguration GetConfiguration(TConfigurationKey key) => _ServiceConfigurations.GetOrAdd(GetConfigurationListKey(), GetMissedConfigurationList).GetValueOrDefault(key);

        protected abstract TKey GetConfigurationListKey();

        protected abstract Dictionary<TConfigurationKey, TConfiguration> GetMissedConfigurationList(TKey key);
    }
}