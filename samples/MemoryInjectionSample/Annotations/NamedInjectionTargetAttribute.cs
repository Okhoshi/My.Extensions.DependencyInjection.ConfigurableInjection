using System;
using My.Extensions.DependencyInjection.ConfigurableInjection.Annotations;

namespace MemoryInjectionSample.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NamedInjectionTargetAttribute : InjectionTargetAttribute
    {
        public NamedInjectionTargetAttribute(string identifier) : base(identifier)
        {
        }

        public string Name { get; set; }
    }
}