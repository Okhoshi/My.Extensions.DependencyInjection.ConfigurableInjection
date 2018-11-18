using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using My.Extensions.DependencyInjection.ConfigurableInjection;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Implementations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public class InjectionConfiguration
    {
        internal Type _ConfigurationKeyType;

        internal Type _ConfigurationType;

        internal Type _ServiceConfigurationProviderInterface => typeof(IServiceConfigurationProvider<,>).MakeGenericType(_ConfigurationKeyType, _ConfigurationType);

        internal Type _TargetInstanceFactoryImpl => typeof(TargetInstanceFactory<>).MakeGenericType(_ConfigurationType);

        internal Type _ServiceConfigurationProviderImpl;

        internal List<Assembly> _LoadedAssemblies = new List<Assembly>();
        
        internal bool _IgnoreInterfacesWithoutClass = false;

        internal bool _ThrowForClassesWithoutInterface = false;

        internal bool _RegisterInjectedOptions = false;

        internal ServiceLifetime _ServiceLifetime = ServiceLifetime.Scoped;

        internal InjectionConfiguration() { }

        public void UseConfigurationProvider<TConfigurationKey, TConfiguration, TProvider>()
            where TConfiguration : ConfigurationBase<TConfigurationKey>
            where TProvider : IServiceConfigurationProvider<TConfigurationKey, TConfiguration>
        {
            _ServiceConfigurationProviderImpl = typeof(TProvider);
            UseConfigurationType<TConfigurationKey, TConfiguration>();
        }

        public void UseConfigurationProvider(Type providerType)
        {
            if (_ConfigurationType == null)
            {
                throw new InvalidOperationException($"Configuration Type not set. Please call {nameof(UseConfigurationType)} before.");
            }

            if (!providerType.GetTypeInfo().ImplementedInterfaces.Contains(_ServiceConfigurationProviderInterface))
            {
                throw new InvalidCastException($"{nameof(providerType)} is not implementing {_ServiceConfigurationProviderInterface.Name}");
            }
            _ServiceConfigurationProviderImpl = providerType;
        }

        private void UseConfigurationType<TConfigurationKey, TConfiguration>()
            where TConfiguration : ConfigurationBase<TConfigurationKey>
        {
            _ConfigurationType = typeof(TConfiguration);
            UseConfigurationKeyType<TConfigurationKey>();
        }

        private void UseConfigurationKeyType<TConfigurationKey>() => _ConfigurationKeyType = typeof(TConfigurationKey);

        public void UseConfigurationType(Type configurationType)
        {
            var baseType = configurationType.GetTypeInfo().BaseType.GetTypeInfo();
            if (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(ConfigurationBase<>))
            {
                throw new InvalidOperationException($"{nameof(configurationType)} must extend {typeof(ConfigurationBase<>)}");
            }

            _ConfigurationType = configurationType;
            _ConfigurationKeyType = baseType.GetGenericArguments()[0];
        }

        //public void AddAssembly(params string[] assemblies) => _LoadedAssemblies.AddRange(assemblies.Select(a => Assembly.Load(a)));

        public void AddAssembly(params Assembly[] assemblies) => _LoadedAssemblies.AddRange(assemblies);

        public void AddAssemblyFromType(params Type[] types) => _LoadedAssemblies.AddRange(types.Select(t => t.GetTypeInfo().Assembly));

        public void AddAssemblyFromType<T>() => AddAssemblyFromType(typeof(T));

        public void IgnoreInterfacesWithoutClass() => _IgnoreInterfacesWithoutClass = true;

        public void ThrowForClassesWithoutInterface() => _ThrowForClassesWithoutInterface = true;

        public void ForceOptionsReconfiguration() => _RegisterInjectedOptions = true;

        public void RegisterAsSingleton() => _ServiceLifetime = ServiceLifetime.Singleton;

        public void RegisterAsScoped() => _ServiceLifetime = ServiceLifetime.Scoped;

        public void RegisterAsTransient() => _ServiceLifetime = ServiceLifetime.Transient;

        internal (TypeInfo[], (TypeInfo cls, TypeInfo ifc)[]) ExtractTypesFromAssemblies()
        {
            var types = _LoadedAssemblies
                .DefaultIfEmpty(Assembly.GetEntryAssembly())
                .SelectMany(a => a.ExportedTypes)
                .Select(t => t.GetTypeInfo())
                .Where(t => t.IsClass || t.IsInterface)
                .Where(t => t.GetCustomAttribute<InjectionTargetAttribute>() != null)
                .ToList();

            if (!types.Any(t => t.IsClass) || !types.Any(t => t.IsInterface))
            {
                throw new InvalidProgramException($"No class or interface with {nameof(InjectionTargetAttribute)} found");
            }

            var interfaces = types.Where(t => t.IsInterface).ToArray();
            var classes = types.Where(t => t.IsClass)
                .SelectMany(c => c.ImplementedInterfaces
                    .Select(i => i.GetTypeInfo())
                    .Where(i => i.GetCustomAttribute<InjectionTargetAttribute>() != null)
                    .Select(i => (cls: c, ifc: i)))
                .ToArray();

            return (interfaces, classes);
        }

        internal void RegisterInterfaces(IServiceCollection services, IEnumerable<TypeInfo> interfaces, IEnumerable<TypeInfo> implementedInterfaces)
        {
            if (!_IgnoreInterfacesWithoutClass)
            {
                var notImplementedInterfaces = interfaces.Except(implementedInterfaces).ToList();
                if (notImplementedInterfaces.Any())
                {
                    throw new InvalidProgramException($"The following interfaces are to be registered but no implementation is available: {string.Join(", ", notImplementedInterfaces)}");
                }
            }

            services.AddSingleton(
                typeof(ITargetInstanceFactory),
                _TargetInstanceFactoryImpl);

            foreach (var i in interfaces.Select(i => i.AsType()))
            {
                services.Add(
                    new ServiceDescriptor(
                        i,
                        sp => sp.GetRequiredService<ITargetInstanceFactory>().GetInstanceFor(i),
                        _ServiceLifetime
                    )
                );
            }
        }

        internal void RegisterClasses(IServiceCollection services, IEnumerable<TypeInfo> classes)
        {
            if (_ThrowForClassesWithoutInterface)
            {
                var notInterfacedImplementations = classes
                    .Where(c => c.ImplementedInterfaces.All(i => i.GetTypeInfo().GetCustomAttribute<InjectionTargetAttribute>() == null)).ToList();
                if (notInterfacedImplementations.Any())
                {
                    throw new InvalidProgramException($"The following classes are available but not reachable: {string.Join(", ", notInterfacedImplementations)}");
                }
            }

            foreach (var c in classes)
            {
                if (_RegisterInjectedOptions 
                 && _ServiceLifetime != ServiceLifetime.Singleton)
                {
                    var optionsTypes = c.DeclaredConstructors.SelectMany(ctor =>
                        ctor.GetParameters()
                            .Select(prm => prm.ParameterType.GetTypeInfo())
                            .Where(pt => pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(IOptions<>))
                    );

                    foreach (var optionsType in optionsTypes)
                    {
                        var managerType = typeof(OptionsManager<>).MakeGenericType(optionsType.GenericTypeArguments[0]);
                        services.Add(new ServiceDescriptor(optionsType.AsType(), managerType, _ServiceLifetime));
                    }
                }

                services.Add(new ServiceDescriptor(c.AsType(), c.AsType(), _ServiceLifetime));
            }
        }
    }
}