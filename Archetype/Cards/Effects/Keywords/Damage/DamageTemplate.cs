using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class DamageTemplate : EffectTemplate
    {
        public override string RulesText => $"Deal {_damage} damage to {Requirements.TargetsText}.";
        public override PromptRequirements Requirements { get; protected set; }

        private int _damage;

        public DamageTemplate(int amount, PromptRequirements requirements)
        {
            _damage = amount;
            Requirements = requirements;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            List<Unit> targets = new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));

            return new DamageEffect(_damage, source, targets);
        }
    }
}
