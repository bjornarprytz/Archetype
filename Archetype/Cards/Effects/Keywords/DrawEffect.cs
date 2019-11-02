namespace Archetype
{
    public class DrawEffect : XEffect, IKeyword
    {
        public override string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawEffect(Unit source, int x)
            : base(source, x, 0, 0)
        {
            Targets.Add(source);
        }

        public DrawEffect(Unit source, int x, int minTargets, int maxTargets)
            : base(source, x, minTargets, maxTargets)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Draw(CardsToDraw + modifier);
        }
    }
}
