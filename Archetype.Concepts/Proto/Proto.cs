using Archetype.Core;

namespace Archetype.Rules.Proto;


public class ProtoCard
{
    public string Name { get; set; } // ID
    public IReadOnlyList<CostInstance> Costs { get; set; }
    public IReadOnlyList<ConditionInstance> Conditions { get; set; }
    public IReadOnlyList<ReactionInstance> Reactions { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
    public IReadOnlyList<FeatureInstance> Features { get; set; }
    public IReadOnlyList<AbilityInstance> Abilities { get; set; }
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }

    public IReadOnlyDictionary<string, string> Characteristics { get; set; }
}

public class ProtoSet
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<ProtoCard> Cards { get; set; }
}
