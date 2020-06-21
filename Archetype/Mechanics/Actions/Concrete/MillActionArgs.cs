
using System;

namespace Archetype
{
    public class MillActionArgs : ParameterizedActionInfo<int>
    {
        public MillActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Mill(Strength);
        }
    }
}
