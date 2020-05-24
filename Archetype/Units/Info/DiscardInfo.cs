namespace Archetype
{
    public class DiscardInfo : ActionInfo
    {
        public DiscardInfo(Unit source, int strength) : base(source, strength)
        {
            Strength += source.SourceModifier.Get<DiscardInfo>();
        }
    }
}
