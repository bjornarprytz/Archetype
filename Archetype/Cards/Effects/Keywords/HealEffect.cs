using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype.Cards.Effects.Keywords
{
    public class HealEffect : XEffect
    {
        public override string Keyword => "Heal";

        public int HealAmount => X;

        internal override string RulesText => $"Heal {X} damage from {Requirements.TargetsText}.";


        public HealEffect(Unit giver, int amount, int minTargets, int maxTargets, Faction targetFaction)
            : base(giver, amount, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            Source.GiveHeal(target, HealAmount);
        }
    }
}
