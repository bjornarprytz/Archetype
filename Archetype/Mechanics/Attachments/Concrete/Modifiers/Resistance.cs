namespace Archetype
{
    public class Resistance : DefensiveActionModifier<Unit, DamageActionArgs>
    {
        public Resistance(int modifier=0, float multiplier=1f) : base(modifier, multiplier)
        {
        }
    }
}
