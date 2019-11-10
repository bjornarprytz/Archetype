using System.Collections.Generic;

namespace Archetype
{
    public class DamageTemplate : EffectTemplate
    {
        public override ChooseTargets Requirements { get; protected set; }

        private int _damage;

        public DamageTemplate(int amount, ChooseTargets requirements) 
            : base (requirements)
        {
            _damage = amount;
        }

        public override Effect CreateEffect(Unit source, Decision userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new DamageEffect(_damage, source, targets);
        }
    }
}
