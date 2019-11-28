using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    class TargetAll<T> : TargetParams<T> where T : GamePiece
    {
        public TargetAll(TargetPredicate<T> predicate)
            : base (predicate)
        {

        }

        internal override Choose<T> GetPrompt(Unit owner, IEnumerable<T> options)
        {
            return new Choose<T>(100, options.Where((p => _predicate(owner, p)))); // TODO: Find a good way to express Choosing 'all', not just many
        }
    }
}
