using System.Collections.Generic;

namespace Archetype
{
    public class MillTemplate : EffectTemplate
    {
        private int _amountToMill;

        public MillTemplate(int amount, TargetParams<Unit> requirements)
            : base (requirements)
        {
            _amountToMill = amount;
        }

        public override Effect CreateEffect(Unit source, List<Unit> targets)
        {
            return new MillEffect(_amountToMill, source, targets);
        }
    }
}
