using System;
using MemoryInjectionSample.Annotations;

namespace MemoryInjectionSample.Interfaces
{
    [NamedInjectionTarget("Dunny", Name = "IDunny")]
    public interface IDunny
    {
        string GetDunny();
    }
}