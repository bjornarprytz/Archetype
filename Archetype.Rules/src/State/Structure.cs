using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Structure : Card, IStructure
{
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