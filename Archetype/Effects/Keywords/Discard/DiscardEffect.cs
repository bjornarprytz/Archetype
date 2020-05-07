
using System.Collections.Generic;

namespace Archetype
{
    public class DiscardEffect : XEffect<Unit>, IKeyword
    {
        public override string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(int damage, EffectArgs args)
            : base(damage, args)
        { }

        protected override void _affect(Unit target, int modifier, IPromptable prompt)
        {
            target.Discard(CardsToDiscard + modifier, prompt);
        }
    }
}
