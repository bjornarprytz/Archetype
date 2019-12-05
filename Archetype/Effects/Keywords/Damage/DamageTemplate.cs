using System.Collections.Generic;

namespace Archetype
{
    public class DamageTemplate : EffectTemplate
    {
        private int _damage;

        public DamageTemplate(int amount, TargetParams<Unit> requirements, TargetOptions targetPool) 
            : base (requirements, targetPool)
        {
            _damage = amount;
        }

        public override Effect CreateEffect(EffectArgs args) => new DamageEffect(_damage, args);
    }
}
