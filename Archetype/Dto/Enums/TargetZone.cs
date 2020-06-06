
namespace Archetype
{
    public enum TargetZone
    {
        // Unit zones
        Battlefield = 1 << 0,
        Graveyard   = 1 << 1,

        // Card zones
        Hand        = 1 << 2,
        DiscardPile = 1 << 3,
        Deck        = 1 << 4,
        Pool        = 1 << 5,


        AnyUnitZone = Battlefield | Graveyard,
        AnyCardZone = Hand | DiscardPile | Deck | Pool,
    }
}
