namespace Archetype
{
    public enum UnitZone
    {
        Any = Battlefield | Graveyard,
        Battlefield = 1 << 0,
        Graveyard   = 1 << 1,

    }  
}
