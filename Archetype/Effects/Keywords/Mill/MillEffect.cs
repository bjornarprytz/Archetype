using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect
    {
        public override string Keyword => "Mill";

        public MillEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affect(Unit target, IPromptable prompt)
        {
            target.Mill(this);
        }
    }
}
