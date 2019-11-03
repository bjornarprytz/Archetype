using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect, IKeyword
    {
        public override string Keyword => "Mill";
        public int CardsToMill => X;

        public MillEffect(Unit source, int x, int minTargets, int maxTargets, Faction targetFaction) 
            : base(source, x, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Mill(CardsToMill + modifier);
        }
    }
}
