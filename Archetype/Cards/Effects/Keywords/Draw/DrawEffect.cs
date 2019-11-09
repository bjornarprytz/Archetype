using System.Collections.Generic;

namespace Archetype
{
    public class DrawEffect : XEffect
    {
        public override string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawEffect(int x, Unit source, List<Unit> targets)
            : base(x, source, targets)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Draw(CardsToDraw + modifier);
        }
    }
}
