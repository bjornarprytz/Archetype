namespace Archetype.Dto.MetaData
{
    public record UnitMetaData
    {
        public string Name { get; init; }
        public string ImageUri { get; init; }
        
        public int Level { get; init; }
    }
}