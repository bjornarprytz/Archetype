using System.Collections.Generic;

namespace Archetype
{
    public class DamageTemplate : EffectTemplate
    {
        private int _damage;

        public DamageTemplate(int amount, TargetParams<Unit> requirements) 
            : base (requirements)
        {
            _damage = amount;
        }

        public override Effect CreateEffect(Unit source, List<Unit> targets)
        {
            return new DamageEffect(_damage, source, targets);
        }
    }
}
