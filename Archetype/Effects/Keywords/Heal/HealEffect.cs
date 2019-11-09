using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class HealEffect : XEffect
    {
        public override string Keyword => "Heal";

        public int HealAmount => X;

        public HealEffect(int x, Unit source, List<Unit> targets)
            : base(x, source, targets)
        { }

        protected override void _affect(Unit target, int modifier, RequiredAction prompt)
        {
            Source.GiveHeal(target, HealAmount);
        }
    }
}
