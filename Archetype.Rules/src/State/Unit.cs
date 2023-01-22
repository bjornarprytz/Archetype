using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Unit : Card, IUnit
{
    // - Target Nodes when played
    // - Enter play in that node.
    // - Can move and attack.
    // - Can be destroyed. Going to graveyard.
    // - Can be modified.
    // - Can have effects that trigger when they enter play, move, attack, or are destroyed or attacked.
    
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