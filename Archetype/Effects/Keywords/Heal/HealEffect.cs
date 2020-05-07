using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class HealEffect : XEffect<Unit>
    {
        public override string Keyword => "Heal";

        public int HealAmount => X;

        public HealEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affect(Unit target, int modifier, IPromptable prompt)
        {
            Source.GiveHeal(target, HealAmount);
        }
    }
}
