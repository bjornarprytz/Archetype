
using System;

namespace Archetype
{
    public interface IHoldCounters
    {
        TypeDictionary<Counter> ActiveCounters { get; }

        public void ApplyCounters<T>(T counter) where T : Counter
        {
            if (!ActiveCounters.Has<T>())
                ActiveCounters.Set<T>(counter);
            else
                ActiveCounters.Get<T>().Combine(counter);
        }

        public void RemoveCounters<T>() where T : Counter
        {
            ActiveCounters.Remove<T>();
        }
    }
}
