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

        protected override void _affect(Unit target, IPromptable prompt)
        {
            target.Heal(this);
        }
    }
}
