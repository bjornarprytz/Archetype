
namespace Archetype
{
    public class DiscardEffect : XEffect, IKeyword
    {
        public string Keyword => "Discard";

        public int CardsToDiscard => X;

        public DiscardEffect(Unit source, Unit target, int x) : base(source, x)
        {
            Targets.Add(target);
        }

        protected override Resolution _resolve => delegate { Source.DealDiscard(this); };
    }
}
