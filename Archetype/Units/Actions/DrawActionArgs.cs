namespace Archetype
{
    public class DrawActionArgs : ActionInfo
    {
        public DrawActionArgs(Unit source, Unit target, int strength) : base(source, target, strength)
        {
        }

        protected override void Resolve()
        {
            (Target as Unit).Draw(Strength);
        }
    }
}
