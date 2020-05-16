using System.Collections.Generic;

namespace Archetype
{
    public class DrawEffect : XEffect
    {
        public DrawEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affect(Unit target, IPromptable prompt)
        {
            target.Draw(this);
        }
    }
}
