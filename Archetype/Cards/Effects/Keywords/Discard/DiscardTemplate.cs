using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class DiscardTemplate : EffectTemplate
    {
        public override string RulesText => $"{Requirements.TargetsText} discards {_cardsToDiscard} cards.";

        public override PromptRequirements Requirements { get; protected set; }

        private int _cardsToDiscard;

        public DiscardTemplate(int amount, PromptRequirements requirements)
        {
            _cardsToDiscard = amount;
            Requirements = requirements;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            List<Unit> targets = new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));

            return new DiscardEffect(_cardsToDiscard, source, targets);
        }
    }
}
