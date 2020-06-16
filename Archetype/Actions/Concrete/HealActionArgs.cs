using System;

namespace Archetype
{
    public class HealActionArgs : ActionInfo
    {
        private Func<int> _getter;
        public int Strength => _getter();
        public HealActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target)
        {
            _getter = getter;
        }

        protected override void Resolve()
        {
            (Target as Unit).Heal(Strength);
        }
    }
}
