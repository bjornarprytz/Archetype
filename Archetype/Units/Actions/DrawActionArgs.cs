namespace Archetype
{
    public class DrawActionArgs : ActionInfo
    {
        public int Strength { get; set; }
        public DrawActionArgs(Unit source, Unit target, int strength) : base(source, target)
        {
            Strength = strength;
        }

        protected override void Resolve()
        {
            (Target as Unit).Draw(Strength);
        }
    }
}
