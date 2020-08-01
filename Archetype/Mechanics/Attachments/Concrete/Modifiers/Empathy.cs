namespace Archetype
{
    public class Empathy : OffensiveActionModifier<Unit, HealActionArgs>
    {
        public Empathy(int modifier=0, float multiplier=1f) : base(modifier, multiplier)
        {
        }
    }
}
