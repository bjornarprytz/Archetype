
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

        protected override void _affectX(Unit target, int amount, IPromptable prompt)
        {
            target.Discard(CardsToDiscard, prompt);

            if (amount < 1) return;

            if (target.Hand.IsEmpty) return;

            if (amount >= target.Hand.Count)
            {
                foreach (Card card in target.Hand) target.Discard(card.Id);
                return;
            }

            Choose<Card> choose = new Choose<Card>(target, amount, target.Hand);

            var response = prompt.PromptImmediate(choose);

            foreach (Card card in response.Choices)
            {
                target.Discard(card.Id);
            }
        }
    }
}
