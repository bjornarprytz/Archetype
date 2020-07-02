using System;

namespace Archetype
{
    public class HealActionArgs : ModifiableActionInfo
    {
        public HealActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Heal(Strength);
        }
    }
}
