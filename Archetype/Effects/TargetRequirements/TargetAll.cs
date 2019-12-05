using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    class TargetAll<T> : TargetParams<T> where T : Unit
    {   
        public TargetAll(TargetPredicate<T> predicate) : base (predicate) { }

        public override int Max => int.MaxValue;
        public override int Min => 0;
    }
}
