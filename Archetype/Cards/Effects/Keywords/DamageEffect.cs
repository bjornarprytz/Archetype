using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect, IKeyword
    {
        public string Keyword => "Damage";
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

        protected override Resolution _resolve => delegate { Source.DealDamage(this); };
    }
}
