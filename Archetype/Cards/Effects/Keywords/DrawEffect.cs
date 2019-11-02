namespace Archetype
{
    public class DrawEffect : XEffect, IKeyword
    {
        public string Keyword => "Draw";
        public int CardsToDraw => X;

        public DrawEffect(Unit source, int x)
            : base(source, x)
        {
            Targets.Add(source);
        }

        public DrawEffect(Unit source, Unit target, int x)
            : base(source, x)
        {
            Targets.Add(target);
        }

        protected override Resolution _resolve => delegate { Source.DealCards(this); };
    }
}
