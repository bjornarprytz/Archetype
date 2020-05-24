
namespace Archetype
{
    public class MillActionArgs : ActionInfo
    {
        public MillActionArgs(Unit source, Unit target, int strength) : base(source, target, strength)
        {
        }

        protected override void Resolve()
        {
            Target.Mill(Strength);
        }
    }
}
