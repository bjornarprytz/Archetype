using Archetype.View.Atoms.MetaData;

namespace Archetype.View.Proto;

public interface IProtoDataFront
{
    string Name { get; }
}

public interface ICardProtoDataFront : IProtoDataFront
{
    string RulesText { get; }
    int Cost { get; }
    int Range { get; }
    CardMetaData MetaData { get; }
}

public interface IUnitProtoDataFront : IProtoDataFront
{
    int Health { get; }
    int Defense { get; }
        
    UnitMetaData BaseMetaData { get; }
}
public interface ICreatureProtoDataFront : IProtoDataFront
{
    string RulesText { get; }
    int Movement { get; }
    int Strength { get; }

    CreatureMetaData MetaData { get; }
}

public interface IStructureProtoDataFront : IProtoDataFront
{
    StructureMetaData MetaData { get; }
    string RulesText { get; }
}
