using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;

namespace Archetype.View.Proto;

public interface IProtoDataFront
{
    string Name { get; }
}

public interface ICardProtoDataFront : IProtoDataFront
{
    int Cost { get; }
    int Range { get; }
    CardMetaData MetaData { get; }
    
    IEnumerable<ITargetDescriptor> TargetDescriptors { get; }
    IEnumerable<IEffectDescriptor> EffectDescriptors { get; }
}

public interface IUnitProtoDataFront : IProtoDataFront
{
    int Health { get; }
    int Defense { get; }
        
    UnitMetaData BaseMetaData { get; }
}
public interface ICreatureProtoDataFront : IProtoDataFront
{
    int Movement { get; }
    int Strength { get; }

    CreatureMetaData MetaData { get; }
}

public interface IStructureProtoDataFront : IProtoDataFront
{
    StructureMetaData MetaData { get; }
    IEnumerable<IEffectDescriptor> EffectDescriptors { get; }
}
