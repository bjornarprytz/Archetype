
namespace Archetype
{
    public class DiscardEffect : XEffect, IKeyword
    {
        public override string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(Unit source, Unit target, int x) : base(source, x)
        {
            Targets.Add(target);
        }

        protected override void _affect(Unit target, int modifier)
        {
            target.Discard(CardsToDiscard + modifier);
        }
    }
}
