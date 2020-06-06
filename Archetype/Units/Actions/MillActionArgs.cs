
namespace Archetype
{
    public class MillActionArgs : ActionInfo
    {
        public int Strength { get; set; }
        public MillActionArgs(Unit source, Unit target, int strength) : base(source, target)
        {
            Strength = strength;
        }

        protected override void Resolve()
        {
            (Target as Unit).Mill(Strength);
        }
    }
}
