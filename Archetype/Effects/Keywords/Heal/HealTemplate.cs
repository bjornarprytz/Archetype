using System.Collections.Generic;

namespace Archetype
{
    public class HealTemplate : EffectTemplate
    {
        public override string RulesText => $"Heal {_amountToHeal} damage from {Requirements.TargetsText}.";

        public override PlayCardPrompt Requirements { get; protected set; }

        private int _amountToHeal;

        public HealTemplate(int amount, PlayCardPrompt requirements)
            : base (requirements)
        {
            _amountToHeal = amount;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new HealEffect(_amountToHeal, source, targets);
        }
    }
}
