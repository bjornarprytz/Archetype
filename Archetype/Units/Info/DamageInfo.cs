
namespace Archetype
{
    public class DamageInfo : ActionInfo
    {

        public DamageInfo(Unit source, int damage) : base(source, damage)
        {
            Strength += source.SourceModifier.Get<DamageInfo>();
        }
    }
}
