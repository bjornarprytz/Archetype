
using System;

namespace Archetype
{
    public interface IHoldCounters
    {
        void Apply<T>(T counter) where T : Counter;
        void Remove<T>() where T : Counter;
    }
}
