using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Structure")]
    public interface IStructure : IUnit, ITriggerSource<IStructure>
    {
        StructureMetaData MetaData { get; }
    }
    
    public class Structure : Unit, IStructure
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