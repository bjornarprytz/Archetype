using System;

namespace Archetype
{
    public class ImmediateValue<T> : ValueDescriptor<T>
    {
        public T Value { get; set; }
        public ImmediateValue(T value)
        {
            Value = value;
        }

        public override Func<T> CreateGetter(Unit source, GameState gameState)
        {
            return () => Value;
        }
    }
}
