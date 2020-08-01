
namespace Archetype
{
    public class Strength : OffensiveActionModifier<Unit, DamageActionArgs>
    {
        public Strength(int modifier=0, float multiplier=1f) : base(modifier, multiplier)
        {
        }
    }
}
