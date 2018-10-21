using System;

namespace MemoryInjectionSample.Interfaces
{
    [InjectionTarget("Dummy")]
    public interface IDummy
    {
        string GetDummy();
    }
}