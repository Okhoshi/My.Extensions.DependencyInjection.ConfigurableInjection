using System;
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
            else if (classToLoad.ImplementationKey == null)
            {
                _Logger.LogError("{targetInterface} ({targetId}) is not correctly configured", targetInterface, targetAttribute.Identifier);
                throw new InvalidOperationException($"{targetInterface} ({targetAttribute.Identifier}) is not correctly configured");
            }

            _Logger.LogTrace("Found {targetClassId} for {targetInterface} ({targetId})", classToLoad.ImplementationKey, targetInterface, targetAttribute.Identifier);
            
            var candidates = _Options.Assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsClass)
                .Where(t => t.GetCustomAttribute<InjectionTargetAttribute>() != null)
                .Where(t => t.GetCustomAttribute<InjectionTargetAttribute>().Identifier == classToLoad.ImplementationKey)
                .ToList();

            if (!candidates.Any())
            {
                _Logger.LogError("No instance referenced by {targetClassId} found", classToLoad.ImplementationKey);
                throw new InvalidOperationException($"No instance referenced by {classToLoad.ImplementationKey} found");
            }
            else if (candidates.Count() > 1)
            {
                _Logger.LogError("More than one instance referenced by {targetClassId} found: {candidates}", classToLoad.ImplementationKey, candidates.Select(t => t.Name));
                throw new InvalidOperationException($"More than one instance referenced by {classToLoad.ImplementationKey} found: {string.Join(", ", candidates.Select(t => t.Name))}");
            }

            var candidateInstance = candidates.Single();
            if (!targetInterface.IsAssignableFrom(candidateInstance))
            {
                _Logger.LogError("The instance referenced by {targetClassId} found does not implement the interface {targetInterface} ({targetInterfaceId})", classToLoad.ImplementationKey, targetInterface, targetAttribute.Identifier);
                throw new InvalidOperationException($"The instance referenced by {classToLoad.ImplementationKey} found does not implement the interface {targetInterface} ({targetAttribute.Identifier})");
            }
            
            return scopeServiceProvider.GetRequiredService(candidateInstance.AsType());
        }

        public ITarget GetInstanceFor<ITarget>()
        {
            return (ITarget)GetInstanceFor(typeof(ITarget));
        }
    }
}