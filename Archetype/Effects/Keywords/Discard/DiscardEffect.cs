
using System.Collections.Generic;

namespace Archetype
{
    public class DiscardEffect : XEffect
    {
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
