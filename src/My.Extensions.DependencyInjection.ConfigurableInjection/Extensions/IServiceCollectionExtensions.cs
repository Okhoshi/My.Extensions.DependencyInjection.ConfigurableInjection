using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurableInjections(this IServiceCollection services, Action<InjectionConfiguration> setUp)
        {
            if (setUp == null)
            {
                throw new NullReferenceException(nameof(setUp));
            }

            var configuration = new InjectionConfiguration();
            setUp(configuration);

            services.Configure<InjectionOptions>(o =>
            {
                o.Assemblies = configuration._LoadedAssemblies.DefaultIfEmpty(Assembly.GetEntryAssembly()).ToArray();
            });

            if (configuration._ServiceConfigurationProviderImpl == null)
            {
                throw new InvalidOperationException($"Configuration Provider not set. Please set it by calling {nameof(InjectionConfiguration.UseConfigurationProvider)}");
            }

            services.AddSingleton(
                configuration._ServiceConfigurationProviderInterface,
                configuration._ServiceConfigurationProviderImpl);

            (var interfaces, var classes) = configuration.ExtractTypesFromAssemblies();

            configuration.RegisterInterfaces(services, interfaces, classes.Select(c => c.ifc).Distinct());
            configuration.RegisterClasses(services, classes.Select(c => c.cls).Distinct());

            return services;
        }

        public static IServiceCollection Configure<TOptions,TConfigureOptions>(this IServiceCollection services)
            where TConfigureOptions : class, IConfigureOptions<TOptions>, new()
            where TOptions : class, new()
        {
            return services.AddSingleton<IConfigureOptions<TOptions>, TConfigureOptions>();
        }
    }
}