using System;

namespace Archetype
{
    public class DiscardActionArgs : ModifiableActionInfo
    {
        public DiscardActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Discard(Strength);
        }
    }
}
