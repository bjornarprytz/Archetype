using System.Collections.Generic;

namespace Archetype
{
    public class HealTemplate : EffectTemplate
    {
        public override ChooseTargets Requirements { get; protected set; }

        private int _amountToHeal;

        public HealTemplate(int amount, ChooseTargets requirements)
            : base (requirements)
        {
            _amountToHeal = amount;
        }

        public override Effect CreateEffect(Unit source, Decision userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new HealEffect(_amountToHeal, source, targets);
        }
    }
}
