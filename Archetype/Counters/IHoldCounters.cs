
using System;

namespace Archetype
{
    public interface IHoldCounters
    {
        TypeDictionary<Counter> ActiveCounters { get; }
        void Apply<T>(T counter) where T : Counter;
        void Remove<T>() where T : Counter;

        // TODO: Move default implementation here (requires C# 8.0):
        /*
         public void Apply<T>(T counter) where T : Counter
        {
            if (!ActiveCounters.Has<T>())
                ActiveCounters.Set<T>(counter);
            else
                ActiveCounters.Get<T>().Combine(counter);
        }

        public void Remove<T>() where T : Counter
        {
            ActiveCounters.Remove<T>();
        }
         */
    }
}
