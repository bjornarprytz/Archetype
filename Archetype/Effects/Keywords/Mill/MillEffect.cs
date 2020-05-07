using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect<Unit>
    {
        public override string Keyword => "Mill";
        public int CardsToMill => X;

        public MillEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affect(Unit target, int modifier, IPromptable prompt)
        {
            target.Mill(CardsToMill + modifier);
        }
    }
}
