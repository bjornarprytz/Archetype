using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public override string Keyword => "Damage";
        public int Damage => X;

        public DamageEffect(Unit attacker, List<Unit> targets, int x) 
            : base(attacker, x)
        {
            Targets = targets;
        }

        public DamageEffect(Unit attacker, Unit target, int x) 
            : base(attacker, x)
        {
            Targets.Add(target);
        }

        protected override void _affect(Unit target, int modifier)
        {
            Source.DealDamage(target, Damage + modifier);
        }
    }
}
