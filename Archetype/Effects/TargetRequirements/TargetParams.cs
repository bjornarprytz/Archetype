using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class TargetParams<T> where T : GamePiece
    {
        protected TargetPredicate<T> _predicate;

        public TargetParams(TargetPredicate<T> predicate)
        {
            _predicate = predicate;
        }

        internal abstract Choose<T> GetPrompt(Unit owner, IEnumerable<T> options);
    }
}
