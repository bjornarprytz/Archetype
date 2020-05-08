using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect
    {
        public override string Keyword => "Mill";

        public MillEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affectX(Unit target, int amount, IPromptable prompt)
        {
            for(int x=0; x < amount; x++)
            {
                Card cardToMill = target.Deck.PeekTop();

                if (cardToMill == null) break;

                cardToMill.MoveTo(target.DiscardPile);
            }
        }
    }
}
