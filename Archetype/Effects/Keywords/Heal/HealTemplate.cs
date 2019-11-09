using System.Collections.Generic;

namespace Archetype
{
    public class HealTemplate : EffectTemplate
    {
        public override string RulesText => $"Heal {_amountToHeal} damage from {Requirements.TargetsText}.";

        public override PromptRequirements Requirements { get; protected set; }

        private int _amountToHeal;

        public HealTemplate(int amount, PromptRequirements requirements)
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
