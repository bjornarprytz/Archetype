using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public override string Keyword => "Damage";

        public DamageEffect(int damage, EffectArgs args) 
            : base(damage, args)
        { }

        protected override void _affectX(Unit target, int amount, IPromptable prompt)
        {
            target.TakeDamage(Source, amount);
            
            
            //Source.DealDamage(target, X);
        }
    }
}
