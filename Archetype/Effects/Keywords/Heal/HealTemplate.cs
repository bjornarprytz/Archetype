using System.Collections.Generic;

namespace Archetype
{
    public class HealTemplate : EffectTemplate
    {
        private int _amountToHeal;

        public HealTemplate(int amount, TargetParams<Unit> requirements, TargetOptions targetPool)
            : base (requirements, targetPool)
        {
            _amountToHeal = amount;
        }

        public override Effect CreateEffect(EffectArgs args) => new HealEffect(_amountToHeal, args);
    }
}
