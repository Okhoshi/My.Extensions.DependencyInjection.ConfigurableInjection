using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection
{
    public interface ITargetInstanceFactory
    {
        object GetInstanceFor(Type targetInterface);

        ITarget GetInstanceFor<ITarget>();
    }
}