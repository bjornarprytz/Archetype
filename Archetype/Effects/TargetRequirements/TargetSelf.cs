
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetSelf : TargetParams<Unit>
    {
        public TargetSelf() : base((s, t) => t.Id == s.Id) { }

        public override int Max => 1;
    }
}
