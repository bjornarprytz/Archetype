using Archetype.Core.Atoms.Base;
using Archetype.Core.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Core.Atoms;

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