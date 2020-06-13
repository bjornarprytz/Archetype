namespace Archetype
{
    public abstract class TriggerData : ITriggerFactory
    {
        // Triggers can be on cards, units or zones.
        // They listen on certain events (TriggerSpring) (e.g. OnDamageTaken, or OnZoneChange)
        // They have certain conditions under which they trigger (e.g. unit life is below 10, or if currentZone == DiscardPile)
        // They if the Spring and the Conditions are in place, an Action should happen


        public abstract ITrigger CreateTrigger();
    }
}
