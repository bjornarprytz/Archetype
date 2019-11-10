using System.Collections.Generic;

namespace Archetype
{
    public class MillTemplate : EffectTemplate
    {
        public override ChooseTargets Requirements { get; protected set; }

        private int _amountToMill;

        public MillTemplate(int amount, ChooseTargets requirements)
            : base (requirements)
        {
            _amountToMill = amount;
        }

        public override Effect CreateEffect(Unit source, Decision userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new MillEffect(_amountToMill, source, targets);
        }
    }
}
