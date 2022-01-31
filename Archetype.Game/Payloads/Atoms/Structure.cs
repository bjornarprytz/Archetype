using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Game.Payloads.Atoms
{
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
}