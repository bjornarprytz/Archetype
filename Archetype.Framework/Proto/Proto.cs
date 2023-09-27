using Archetype.Framework.Definitions;

namespace Archetype.Framework.Proto;

public class ProtoAbility
{
    public string Name { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
    public IReadOnlyList<ConditionInstance> Conditions { get; set; }
    public IReadOnlyList<CostInstance> Costs { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }
}

public class ProtoCard
{
    public string Name { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
    public IReadOnlyList<ConditionInstance> Conditions { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
    public IReadOnlyList<CostInstance> Costs { get; set; }
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }
    public IReadOnlyDictionary<string, ProtoAbility> Abilities { get; set; } // TODO: This needs to account for static keywords like cost and condition, which is not part of the ability
    
    public IReadOnlyList<ReactionInstance> Reactions { get; set; }
    public IReadOnlyDictionary<string, CharacteristicInstance> Characteristics { get; set; }
}

public class ProtoSet
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<ProtoCard> Cards { get; set; }
}
