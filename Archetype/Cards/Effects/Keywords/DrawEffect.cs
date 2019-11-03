namespace Archetype
{
    public class DrawEffect : XEffect, IKeyword
    {
        public override string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawEffect(Unit source, int x, Faction targetFaction)
            : base(source, x, 0, 0, targetFaction)
        {
            Targets.Add(source);
        }

        public DrawEffect(Unit source, int x, int minTargets, int maxTargets, Faction targetFaction)
            : base(source, x, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Draw(CardsToDraw + modifier);
        }
    }
}
