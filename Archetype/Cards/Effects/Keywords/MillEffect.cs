using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect, IKeyword
    {
        public override string Keyword => "Mill";
        public int CardsToMill => X;

        internal override string RulesText => $"Mill {Requirements.TargetsText} for {X} card(s)";

        public MillEffect(int x, int minTargets, int maxTargets, Faction targetFaction) 
            : base(x, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Mill(CardsToMill + modifier);
        }
    }
}
