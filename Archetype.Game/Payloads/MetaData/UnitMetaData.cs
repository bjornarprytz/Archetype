namespace Archetype.Game.Payloads.MetaData
{
    public record StructureMetaData(string Name, string ImageUri, int Level) 
        : UnitMetaData(Name, ImageUri, Level);
    public record CreatureMetaData(string Name, string ImageUri, int Level) 
        : UnitMetaData(Name, ImageUri, Level);

    public record UnitMetaData(string Name, string ImageUri, int Level);
}