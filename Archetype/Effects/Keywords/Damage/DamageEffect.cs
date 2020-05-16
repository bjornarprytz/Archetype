using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public DamageEffect(int damage, EffectArgs args) 
            : base(damage, args)
        { }

        protected override void _affect(Unit target, IPromptable prompt)
        {
            target.Damage(this);
        }
    }
}
