using Archetype.Game.Attributes;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Structure")]
    public interface IStructure : IUnit
    {
        StructureMetaData MetaData { get; }
    }
    
    public class Structure : Unit, IStructure
    {
        public Structure(IStructureProtoData protoData, IGameAtom owner) : base(protoData, owner)
        {
            MetaData = protoData.MetaData;

        }

        public StructureMetaData MetaData { get; }

        public override UnitMetaData BaseMetaData => MetaData;
    }
}