namespace Archetype.View.Atoms.MetaData;

public record StructureMetaData : UnitMetaData
{
    
}

public record CreatureMetaData : UnitMetaData
{
    
}

public record UnitMetaData
{
    public string SetName { get; init; }
    public string ImageUri { get; init; }
    public int Level { get; init; }
}