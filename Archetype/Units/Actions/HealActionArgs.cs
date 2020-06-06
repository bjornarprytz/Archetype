namespace Archetype
{
    public class HealActionArgs : ActionInfo
    {
        public int Strength { get; set; }
        public HealActionArgs(Unit source, Unit target, int strength) : base(source, target)
        {
            Strength = strength;
        }

        protected override void Resolve()
        {
            (Target as Unit).Heal(Strength);
        }
    }
}
