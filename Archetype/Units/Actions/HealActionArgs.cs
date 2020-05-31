namespace Archetype
{
    public class HealActionArgs : ActionInfo
    {
        public HealActionArgs(Unit source, Unit target, int strength) : base(source, target, strength)
        {
        }

        protected override void Resolve()
        {
            (Target as Unit).Heal(Strength);
        }
    }
}
