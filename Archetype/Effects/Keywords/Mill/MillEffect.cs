using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect
    {
        public override string Keyword => "Mill";
        public int CardsToMill => X;

        public MillEffect(int x, Unit source, List<Unit> targets)
            : base(x, source, targets)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Mill(CardsToMill + modifier);
        }
    }
}
