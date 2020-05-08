using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class HealEffect : XEffect
    {
        public override string Keyword => "Heal";

        public HealEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affectX(Unit target, int amount, IPromptable prompt)
        {
            target.Resources.Gain(new Payment<Life>(amount));
        }
    }
}
