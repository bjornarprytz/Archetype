using System.Collections.Generic;

namespace Archetype
{
    public class DrawEffect : XEffect
    {
        public override string Keyword => "Draw";
        public DrawEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affectX(Unit target, int amount, IPromptable prompt)
        {
            target.Draw(amount);
        }
    }
}
