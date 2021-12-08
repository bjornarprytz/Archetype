namespace Archetype.Game.Payloads.MetaData
{
    public record UnitMetaData
    {
        public string Name { get; init; }
        public string ImageUri { get; init; }
        
        public int Level { get; init; } // This should be used to manage wave difficulty. NOTE: Might be replaced with a more deterministic point system on the stats
    }
}