using System;

namespace Archetype
{
    public abstract class ValueDescriptor<T>
    {
        public abstract Func<T> CreateGetter(Unit source, GameState gameState);
    }
}
