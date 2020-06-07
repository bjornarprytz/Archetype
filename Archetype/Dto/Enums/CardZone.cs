
namespace Archetype
{
    public enum CardZone
    {
        Any = Hand | DiscardPile | Deck | Pool,
        Hand        = 1 << 1,
        DiscardPile = 1 << 2,
        Deck        = 1 << 3,
        Pool        = 1 << 4,
    }
}
