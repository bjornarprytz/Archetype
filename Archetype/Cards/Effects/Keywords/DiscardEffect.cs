
namespace Archetype
{
    public class DiscardEffect : XEffect, IKeyword
    {
        public override string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(Unit source, Unit target, int x, int minTargets, int maxTargets, Faction targetFaction) 
            : base(source, x, minTargets, maxTargets, targetFaction)
        { }

        protected override void _affect(Unit target, int modifier, DecisionPrompt prompt)
        {
            target.Discard(CardsToDiscard + modifier, prompt);
        }
    }
}
