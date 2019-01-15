using System.Collections.Generic;

namespace My.Extensions.DependencyInjection.ConfigurableInjection
{
    public abstract class ConfigurationBase<TConfigurationKey>
    {
        public TConfigurationKey InterfaceKey { get; set; }

        public IEnumerable<TConfigurationKey> ImplementationKeys { get; set; }
    }
}