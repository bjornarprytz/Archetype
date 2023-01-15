using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Unit : Card, IUnit
{
    public Unit(IProtoUnit protoUnit) : base(protoUnit)
    {
        Power = protoUnit.UnitStats.Power;
        Movement = protoUnit.UnitStats.Movement;
        CurrentHealth = MaxHealth = protoUnit.UnitStats.Health;
    }
    
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Power { get; set; }
    public int Movement { get; set; }
}