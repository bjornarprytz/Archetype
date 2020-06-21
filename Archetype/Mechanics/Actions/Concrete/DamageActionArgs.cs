
using System;

namespace Archetype
{
    public class DamageActionArgs : ParameterizedActionInfo<int>
    {
        public DamageActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Damage(Strength);
        }
    }
}
