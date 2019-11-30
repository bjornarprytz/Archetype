using System;
using System.Collections.Generic;

namespace Archetype
{
    public abstract class ManualSelect<T> : TargetParams<T> where T : GamePiece
    {
        public ManualSelect(TargetPredicate<T> predicate) : base(predicate) { }

        public abstract Choose<T> GetPrompt(Unit owner, IEnumerable<T> options);
    }
}
