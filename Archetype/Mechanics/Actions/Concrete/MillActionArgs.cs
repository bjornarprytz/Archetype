
using System;

namespace Archetype
{
    public class MillActionArgs : ModifiableActionInfo
    {
        public MillActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Mill(Strength);
        }
    }
}
