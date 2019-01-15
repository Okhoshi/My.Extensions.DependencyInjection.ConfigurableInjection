using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;
using My.Extensions.DependencyInjection.ConfigurableInjection.Internal;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Implementations
{
    using TConfigurationKey = System.String;

    internal class TargetInstanceFactory<TConfiguration> : ITargetInstanceFactory
        where TConfiguration : ConfigurationBase<TConfigurationKey>
    {
        private readonly ILogger<TargetInstanceFactory<TConfiguration>> _Logger;
        private readonly IServiceConfigurationProvider<TConfigurationKey, TConfiguration> _ConfigurationProvider;
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly InjectionOptions _Options;

        public TargetInstanceFactory(IOptions<InjectionOptions> options, ILogger<TargetInstanceFactory<TConfiguration>> logger, IServiceConfigurationProvider<TConfigurationKey, TConfiguration> configurationProvider, IHttpContextAccessor httpContextAccessor)
        {
            _Options = options.Value;
            _Logger = logger;
            _ConfigurationProvider = configurationProvider;
            _HttpContextAccessor = httpContextAccessor;
        }

        public object GetInstanceFor(Type target)
        {
            var scopeServiceProvider = _HttpContextAccessor.HttpContext.RequestServices;

            var targetInterface = target.GetTypeInfo();
            var isEnumerationTarget = false;
            if(targetInterface.IsGenericType)
            {
                var enumeratedTarget = targetInterface.GetGenericTypeDefinition().GetTypeInfo();
                if (enumeratedTarget.ImplementedInterfaces.Contains(typeof(IEnumerable)))
                {
                    targetInterface = targetInterface.GetGenericArguments().First().GetTypeInfo();
                    isEnumerationTarget = true;
                }
            }

            var targetAttribute = targetInterface.GetCustomAttribute<InjectionTargetAttribute>();
            if (targetAttribute == null)
            {
                _Logger.LogError($"{nameof(targetInterface)} ({{targetInterface}}) is not annotated with {nameof(InjectionTargetAttribute)}.", targetInterface);
                throw new InvalidOperationException($"{nameof(targetInterface)} is not annotated with {nameof(InjectionTargetAttribute)}.");
            }

            _Logger.LogTrace("Getting instance for {interface} with Id {id}", targetInterface.Name, targetAttribute.Identifier);

            var classToLoad = _ConfigurationProvider.GetConfiguration(targetAttribute.Identifier);
            if (classToLoad == null)
            {
                _Logger.LogError("{targetInterface} ({targetId}) was not found in the configuration", targetInterface, targetAttribute.Identifier);
                throw new InvalidOperationException($"{targetInterface} ({targetAttribute.Identifier}) was not found in the configuration");
            }
            else if (classToLoad.ImplementationKeys == null || classToLoad.ImplementationKeys.Count() == 0)
            {
                _Logger.LogError("{targetInterface} ({targetId}) is not correctly configured", targetInterface, targetAttribute.Identifier);
                throw new InvalidOperationException($"{targetInterface} ({targetAttribute.Identifier}) is not correctly configured");
            }

            _Logger.LogTrace("Found {targetClassId} for {targetInterface} ({targetId})", classToLoad.ImplementationKeys, targetInterface, targetAttribute.Identifier);

            var implementationsToLoad = classToLoad.ImplementationKeys.ToList();
            if (!isEnumerationTarget)
            {
                implementationsToLoad = classToLoad.ImplementationKeys.Take(1).ToList();
            }

            var candidates = _Options.Assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsClass)
                .Where(t => t.GetCustomAttribute<InjectionTargetAttribute>() != null)
                .Where(t => implementationsToLoad.Contains(t.GetCustomAttribute<InjectionTargetAttribute>().Identifier))
                .OrderBy(t => implementationsToLoad.IndexOf(t.GetCustomAttribute<InjectionTargetAttribute>().Identifier))
                .ToList();

            if (!candidates.Any())
            {
                _Logger.LogError("No instance referenced by {targetClassId} found", string.Join(", ", implementationsToLoad));
                throw new InvalidOperationException($"No instance referenced by {string.Join(", ", implementationsToLoad)} found");
            }
            else if (!isEnumerationTarget && candidates.Count() > 1)
            {
                _Logger.LogError("More than one instance referenced by {targetClassId} found: {candidates}", string.Join(", ", implementationsToLoad), candidates.Select(t => t.Name));
                throw new InvalidOperationException($"More than one instance referenced by {string.Join(", ", implementationsToLoad)} found: {string.Join(", ", candidates.Select(t => t.Name))}");
            }

            var instances = candidates.Select(candidateInstance => {
                if (!targetInterface.IsAssignableFrom(candidateInstance))
                {
                    _Logger.LogError("The instance referenced by {targetClassId} found does not implement the interface {targetInterface} ({targetInterfaceId})", string.Join(", ", implementationsToLoad), targetInterface, targetAttribute.Identifier);
                    throw new InvalidOperationException($"The instance referenced by {string.Join(", ", implementationsToLoad)} found does not implement the interface {targetInterface} ({targetAttribute.Identifier})");
                }
                
                return scopeServiceProvider.GetRequiredService(candidateInstance.AsType());
            });

            var cast = typeof(Enumerable).GetTypeInfo().GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(targetInterface.AsType());
            var toList = typeof(Enumerable).GetTypeInfo().GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(targetInterface.AsType());

            return isEnumerationTarget 
                ? toList.Invoke(null, new[]{cast.Invoke(null, new[] {instances})}) 
                : instances.Single();
        }

        public ITarget GetInstanceFor<ITarget>()
        {
            return (ITarget)GetInstanceFor(typeof(ITarget));
        }
    }
}