using System.Collections.Generic;

namespace Archetype
{
    public class MillTemplate : EffectTemplate
    {
        public override string RulesText => $"Mill {Requirements.TargetsText} for {_amountToMill} card(s)";

        public override PlayCardPrompt Requirements { get; protected set; }

        private int _amountToMill;

        public MillTemplate(int amount, PlayCardPrompt requirements)
            : base (requirements)
        {
            _amountToMill = amount;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new MillEffect(_amountToMill, source, targets);
        }
    }
}
