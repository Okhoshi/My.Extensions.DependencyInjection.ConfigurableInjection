﻿using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class InjectionTargetAttribute : System.Attribute
    {
        private readonly string identifier;
        
        // This is a positional argument
        public InjectionTargetAttribute(string identifier)
        {
            this.identifier = identifier;
        }
        
        public string Identifier
        {
            get { return identifier; }
        }
        
        public string Name { get; set; }

        public string Description { get; set; }
    }
}