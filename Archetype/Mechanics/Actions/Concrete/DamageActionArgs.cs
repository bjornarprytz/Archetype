
using System;

namespace Archetype
{
    public class DamageActionArgs : ActionInfo
    {
        private Func<int> _getter { get; set; }
        public int Strength => _getter();

        public DamageActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target)
        {
            _getter = getter;
        }

        protected override void Resolve()
        {
            (Target as Unit).Damage(Strength);
        }
    }
}
