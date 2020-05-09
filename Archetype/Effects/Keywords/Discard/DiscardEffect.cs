
using System.Collections.Generic;

namespace Archetype
{
    public class DiscardEffect : XEffect, IKeyword
    {
        public override string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(int damage, EffectArgs args)
            : base(damage, args)
        { }

        protected override void _affect(Unit target, IPromptable prompt)
        {
            target.Discard(this, prompt);
        }
    }
}
