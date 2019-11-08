namespace Archetype
{
    public class DrawEffect : XEffect
    {
        public override string Keyword => "Draw";
        public int CardsToDraw => X;

        internal override string RulesText => $"{Requirements.TargetsText} draw(s) {X} card(s)";

        public DrawEffect(int x, Faction targetFaction)
            : base(x, 0, 0, targetFaction)
        { }

        public DrawEffect(int x, int minTargets, int maxTargets, Faction targetFaction)
            : base(x, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Draw(CardsToDraw + modifier);
        }
    }
}
