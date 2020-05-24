namespace Archetype
{
    public class HealInfo : ActionInfo
    {
        public HealInfo(Unit source, int strength) : base(source, strength)
        {
            Strength += source.SourceModifier.Get<HealInfo>();
        }
    }
}
