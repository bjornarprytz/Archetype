using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public override string Keyword => "Damage";
        public int Damage => X;

        internal override string RulesText => $"Deal {X} damage to {Requirements.TargetsText}.";

        public DamageEffect(Unit attacker, int damage, int minTargets, int maxTargets, Faction targetFaction) 
            : base(attacker, damage, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            Source.DealDamage(target, Damage + modifier);
        }
    }
}
