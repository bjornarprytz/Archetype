using System.Collections.Generic;

namespace Archetype
{
    public class DrawEffect : XEffect<Unit>
    {
        public override string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawEffect(int x, EffectArgs args)
            : base(x, args)
        { }

        protected override void _affect(Unit target, int modifier, IPromptable prompt)
        {
            target.Draw(CardsToDraw + modifier);
        }
    }
}
