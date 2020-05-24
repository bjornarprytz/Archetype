
namespace Archetype
{
    public class MillInfo : ActionInfo
    {
        public MillInfo(Unit source, int strength) : base(source, strength)
        {
            Strength += source.SourceModifier.Get<MillInfo>();
        }
    }
}
