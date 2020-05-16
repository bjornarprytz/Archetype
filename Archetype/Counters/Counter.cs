
using System;

namespace Archetype
{
    public abstract class Counter<T> : Counter, IOwned<T> where T : GamePiece, IHoldCounters
    {
        public Type TargetType => typeof(T);

        public T Owner { get; private set; }
    }

    public abstract class Counter
    {
        public int Charges { get; set; }

        public Counter(int initialCharges=0)
        {
            Charges = initialCharges;
        }

        public abstract void Start();
        public abstract void Stop();
        public abstract void Tick();
        public abstract void Combine(Counter other);
    }
}
