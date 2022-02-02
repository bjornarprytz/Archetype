using Archetype.Core.Play;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Play;
using Archetype.View.Proto;

namespace Archetype.Core.Proto;

public interface IUnitProtoData : IUnitProtoDataFront { }

public interface ICreatureProtoData : IUnitProtoData, ICreatureProtoDataFront { }

public interface IStructureProtoData : IUnitProtoData, IStructureProtoDataFront
{
    IEnumerable<IEffect> Effects { get; }
}

public class CreatureProtoData : UnitProtoData, ICreatureProtoData
{
    public int Movement { get; set; }
    public int Strength { get; set; }
    public CreatureMetaData MetaData { get; set; }
    public override UnitMetaData BaseMetaData => MetaData;
}

public class StructureProtoData : UnitProtoData, IStructureProtoData
{
    private readonly List<IEffect> _effects;

    public StructureProtoData(List<IEffect> effects)
    {
        _effects = effects;
    }
        
    public StructureMetaData MetaData { get; set; }
    public IEnumerable<IEffectDescriptor> EffectDescriptors { get; set; }
    public override UnitMetaData BaseMetaData => MetaData;
    public IEnumerable<IEffect> Effects => _effects;
}

public abstract class UnitProtoData : ProtoData, IUnitProtoData
{
    public int Health { get; set; }
    public int Defense { get; set; }
    public abstract UnitMetaData BaseMetaData { get; }
}