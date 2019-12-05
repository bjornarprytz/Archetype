using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class TargetParams<T> where T : GamePiece
    {
        protected TargetPredicate<T> _predicate;

        protected TargetParams(TargetPredicate<T> predicate)
        {
            _predicate = predicate;
        }

        public EffectArgs GetArgs(Unit owner, IEnumerable<T> options) => new EffectArgs(Min, Max, ValidTargets(owner, options));

        public abstract int Max { get; }
        public virtual int Min => 0;

        internal bool HasEnoughTargets(Unit owner, IEnumerable<T> options) => ValidTargets(owner, options).Count() >= Min;
        internal bool HasValidTargets(Unit owner, IEnumerable<T> options) => options.Any(p => _predicate(owner, p));
        internal IEnumerable<T> ValidTargets(Unit owner, IEnumerable<T> options) => options.Where(p => _predicate(owner, p));
        
    }
}
