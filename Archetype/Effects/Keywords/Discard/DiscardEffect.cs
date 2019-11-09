
using System.Collections.Generic;

namespace Archetype
{
    public class DiscardEffect : XEffect, IKeyword
    {
        public override string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(int damage, Unit source, List<Unit> targets = null)
            : base(damage, source, targets)
        { }

        protected override void _affect(Unit target, int modifier, RequiredAction prompt)
        {
            target.Discard(CardsToDiscard + modifier, prompt);
        }
    }
}
