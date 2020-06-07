
namespace Archetype
{
    public enum UnitZone
    {
        // Unit zones
        Battlefield = 1 << 0,
        Graveyard   = 1 << 1,

        Any = Battlefield | Graveyard,
    }
}
