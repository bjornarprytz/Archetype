using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect<Unit>
    {
        public override string Keyword => "Damage";

        public DamageEffect(int damage, EffectArgs args) 
            : base(damage, args)
        { }

        protected override void _affect(Unit target, int modifier, RequiredAction prompt)
        {
            Source.DealDamage(target, X + modifier);
        }
    }
}
