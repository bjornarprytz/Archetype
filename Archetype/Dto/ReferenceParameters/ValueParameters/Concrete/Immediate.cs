using System;

namespace Archetype
{
    public class Immediate<T> : ValueDescriptor<T>
    {
        public T Value { get; set; }
        public Immediate(T value)
        {
            Value = value;
        }

        public override Func<T> CreateGetter(Unit source, GameState gameState)
        {
            return () => Value;
        }
    }
}
