using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public override string Keyword => "Damage";
        public int Damage => X;

        public DamageEffect(Unit attacker, int damage, int minTargets, int maxTargets) 
            : base(attacker, damage, minTargets, maxTargets)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            Source.DealDamage(target, Damage + modifier);
        }
    }
}
