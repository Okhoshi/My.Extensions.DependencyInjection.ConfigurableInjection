using System;
using MemoryInjectionSample.Annotations;

namespace MemoryInjectionSample.Interfaces
{
    [NamedInjectionTarget("Dummy", Name = "IDummy")]
    public interface IDummy
    {
        string GetDummy();
    }
}