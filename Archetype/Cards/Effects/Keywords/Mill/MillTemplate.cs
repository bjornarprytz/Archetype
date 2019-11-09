using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class MillTemplate : EffectTemplate
    {
        public override string RulesText => $"Mill {Requirements.TargetsText} for {_amountToMill} card(s)";

        public override PromptRequirements Requirements { get; protected set; }

        private int _amountToMill;

        public MillTemplate(int amount, PromptRequirements requirements)
        {
            _amountToMill = amount;
            Requirements = requirements;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            List<Unit> targets = new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));

            return new MillEffect(_amountToMill, source, targets);
        }
    }
}
