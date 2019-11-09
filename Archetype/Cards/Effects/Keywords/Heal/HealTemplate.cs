using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class HealTemplate : EffectTemplate
    {
        public override string RulesText => $"Heal {_amountToHeal} damage from {Requirements.TargetsText}.";

        public override PromptRequirements Requirements { get; protected set; }

        private int _amountToHeal;

        public HealTemplate(int amount, PromptRequirements requirements)
        {
            _amountToHeal = amount;
            Requirements = requirements;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            List<Unit> targets = new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));

            return new HealEffect(_amountToHeal, source, targets);
        }
    }
}
