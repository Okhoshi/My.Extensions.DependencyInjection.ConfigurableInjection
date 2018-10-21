namespace My.Extensions.DependencyInjection.ConfigurableInjection
{
    public abstract class ConfigurationBase<TConfigurationKey>
    {
        public TConfigurationKey InterfaceKey { get; set; }

        public TConfigurationKey ImplementationKey { get; set; }
    }
}