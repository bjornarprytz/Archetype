
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    class TargetSelf : TargetParams<Unit>
    {
        public TargetSelf() : base((s, t) => t.Id == s.Id) { }

        internal override Choose<Unit> GetPrompt(Unit owner, IEnumerable<Unit> options)
        {
            return new Choose<Unit>(1, options.Where((p => _predicate(owner, p))));
        }
    }
}
