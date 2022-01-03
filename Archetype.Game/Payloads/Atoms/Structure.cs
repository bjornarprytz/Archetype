using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Game.Payloads.Atoms
{
    [Target("Structure")]
    public interface IStructure : IUnit, ITriggerSource<IStructure>, IStructureFront
    {
        
    }

    internal class Structure : Unit, IStructure
    {
        private readonly List<IEffect<ITriggerContext<IStructure>>> _effects;
        
        public Structure(IStructureProtoData protoData, IGameAtom owner) : base(protoData, owner)
        {
            MetaData = protoData.MetaData;
            _effects = protoData.Effects.ToList(); // TODO: Maybe just use the same enumeration
        }

        public StructureMetaData MetaData { get; }
        public IEnumerable<IEffect<ITriggerContext<IStructure>>> Effects => _effects;

        public override UnitMetaData BaseMetaData => MetaData;
    }
}