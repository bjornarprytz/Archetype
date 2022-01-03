using Archetype.View.Atoms.MetaData;

namespace Archetype.View.Proto;

public interface ICardProtoDataFront
{
    string RulesText { get; }
    int Cost { get; }
    int Range { get; }
    CardMetaData MetaData { get; }
}

public interface IUnitProtoDataFront
{
    int Health { get; }
    int Defense { get; }
        
    UnitMetaData BaseMetaData { get; }
}
public interface ICreatureProtoDataFront
{
    string RulesText { get; }
    int Movement { get; }
    int Strength { get; }

    CreatureMetaData MetaData { get; }
}

public interface IStructureProtoDataFront
{
    StructureMetaData MetaData { get; }
    string RulesText { get; }
}
