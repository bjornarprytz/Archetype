using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Structure : Card, IStructure
{
    // - Target Nodes when played
    // - Enter play in that node.
    // - Can be destroyed. Does not go to graveyard, but maybe leaves a token of resources for the player?
    // - Can be modified.
    // - Can have effects that trigger when are destroyed or attacked.
    // - Can have effects that trigger when a unit enters play in the same node.
    // - Can have static effects that modify units in the same node, or globally.
    
    public Structure(IProtoStructure protoStructure) : base(protoStructure)
    {
        Power = protoStructure.StructureStats.Power;
        CurrentHealth = MaxHealth = protoStructure.StructureStats.Health;
        Slots = protoStructure.StructureStats.Slots;
    }

    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Power { get; set; }
    public int Slots { get; set; }
}