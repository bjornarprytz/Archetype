namespace Archetype
{
    public class DrawCards : XEffect, IKeyword
    {
        public string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawCards(Unit source, int x)
            : base(source, x)
        {
            Targets.Add(source);
        }

        public DrawCards(Unit source, Unit target, int x)
            : base(source, x)
        {
            Targets.Add(target);
        }

        protected override Resolution _resolve => delegate { Source.DealCards(this); };
    }
}
