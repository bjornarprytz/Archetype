using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Game.Payloads.Atoms
{
    public interface ICreature : IUnit, ICreatureFront
    {
        
    }
    
    internal class Creature : Unit, ICreature
    {
        public Creature(ICreatureProtoData protoData) : base(protoData)
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