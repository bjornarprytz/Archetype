using System.Collections.Generic;

namespace Archetype
{
    public class DamageEffect : XEffect
    {
        public override string Keyword => "Damage";

        public DamageEffect(int damage, Unit source, List<Unit> targets=null) 
            : base(damage, source, targets)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            Source.DealDamage(target, X + modifier);
        }
    }
}
