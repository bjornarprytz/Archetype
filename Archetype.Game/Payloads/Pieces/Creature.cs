using Archetype.Game.Attributes;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICreatureFront : IUnitFront
    {
        CreatureMetaData MetaData { get; }
        int Strength { get; }
        int Movement { get; }
    }

    [Target("Creature")]
    internal interface ICreature : IUnit, ICreatureFront
    {
        
    }
    
    internal class Creature : Unit, ICreature
    {
        public Creature(ICreatureProtoData protoData, IGameAtom owner) : base(protoData, owner)
        {
            MetaData = protoData.MetaData;
            Strength = protoData.Strength;
            Movement = protoData.Movement;
        }

        public int Strength { get; private set; }
        public int Movement { get; private set; }


        public CreatureMetaData MetaData { get; }
        
        public override UnitMetaData BaseMetaData => MetaData;
    }
}