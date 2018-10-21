using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Internal
{
    internal interface ITargetInstanceFactory
    {
        object GetInstanceFor(Type targetInterface);

        ITarget GetInstanceFor<ITarget>();
    }
}