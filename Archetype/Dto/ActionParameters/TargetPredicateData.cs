using System;
using System.Collections.Generic;

namespace Archetype
{
    public abstract class TargetPredicateData
    {
        public abstract IEnumerable<ITarget> GetOptions(Unit source, GameState gameState);
    }
}
