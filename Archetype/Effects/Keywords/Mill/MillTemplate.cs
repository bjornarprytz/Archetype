using System.Collections.Generic;

namespace Archetype
{
    public class MillTemplate : EffectTemplate
    {
        private int _amountToMill;

        public MillTemplate(int amount, TargetParams<Unit> requirements, TargetOptions targetPool)
            : base (requirements, targetPool)
        {
            _amountToMill = amount;
        }

        public override Effect CreateEffect(EffectArgs args) => new MillEffect(_amountToMill, args);
    }
}
