using System.Collections.Generic;

namespace Archetype
{
    public class DiscardTemplate : EffectTemplate
    {
        public override string RulesText => $"{Requirements.TargetsText} discards {_cardsToDiscard} cards.";

        public override PlayCardPrompt Requirements { get; protected set; }

        private int _cardsToDiscard;

        public DiscardTemplate(int amount, PlayCardPrompt requirements)
            : base(requirements)
        {
            _cardsToDiscard = amount;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new DiscardEffect(_cardsToDiscard, source, targets);
        }
    }
}
