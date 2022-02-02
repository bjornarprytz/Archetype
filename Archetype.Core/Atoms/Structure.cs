using Archetype.Core.Atoms.Base;
using Archetype.Core.Play;
using Archetype.Core.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Core.Atoms;

public interface IStructure : IUnit, IStructureFront, IEffectProvider
{
        
}

internal class Structure : Unit, IStructure
{
    private readonly List<IEffect> _effects;
        
    public Structure(IStructureProtoData protoData) : base(protoData)
    {
        MetaData = protoData.MetaData;
        _effects = protoData.Effects.ToList(); // TODO: Maybe just use the same enumeration
    }

    public StructureMetaData MetaData { get; }
    public IEnumerable<IEffect> Effects => _effects;

    public override UnitMetaData BaseMetaData => MetaData;
}