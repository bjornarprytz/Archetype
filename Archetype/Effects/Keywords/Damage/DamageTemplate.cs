using System.Collections.Generic;

namespace Archetype
{
    public class DamageTemplate : EffectTemplate
    {
        public override string RulesText => $"Deal {_damage} damage to {Requirements.TargetsText}.";
        public override PromptRequirements Requirements { get; protected set; }

        private int _damage;

        public DamageTemplate(int amount, PromptRequirements requirements) 
            : base (requirements)
        {
            _damage = amount;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new DamageEffect(_damage, source, targets);
        }
    }
}
