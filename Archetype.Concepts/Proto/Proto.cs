using Archetype.Core;

namespace Archetype.Rules.Proto;

public abstract class ProtoData
{
    public string Keyword { get; set; }
}

public class ProtoEffect : ProtoData
{
    public CreateEffect Create { get; set; }
}

public class ProtoReaction : ProtoData
{
    public ProtoEffect Effect { get; set; }
}

public class ProtoAura : ProtoData
{
    public ProtoCondition Condition { get; set; }
}

public class ProtoFeature : ProtoData
{
    public int Stacks { get; set; }
}

public class ProtoAbility : ProtoData
{
    public IReadOnlyList<ProtoCondition> Conditions { get; set; }
    public IReadOnlyList<ProtoCost> Costs { get; set; }
    public IReadOnlyList<ProtoEffect> Effects { get; set; }
    
    public CreateAbilityEffects CreateEffects { get; set; }
}

public class ProtoCondition : ProtoData
{
    public CheckState Check { get; set; } 
}

public class ProtoCost : ProtoData
{
    public int Amount { get; set; }
}

public class ProtoComputedProperty : ProtoData
{
    public string Key { get; set; }
    public ComputeProperty Compute { get; set; }
}

public class ProtoCard
{
    public string Name { get; set; } // ID
    public CardType Type { get; set; }
    public IReadOnlyList<ProtoCost> Costs { get; set; }
    public IReadOnlyList<ProtoCondition> Conditions { get; set; }
    public IReadOnlyList<ProtoReaction> Reactions { get; set; }
    public IReadOnlyList<ProtoEffect> Effects { get; set; }
    public IReadOnlyList<ProtoAura> Auras { get; set; }
    public IReadOnlyList<ProtoFeature> Features { get; set; }
    public IReadOnlyList<ProtoAbility> Abilities { get; set; }
    public IReadOnlyList<ProtoComputedProperty> ComputedProperties { get; set; }

    public IReadOnlyDictionary<string, string> Characteristics { get; set; }
}

public class ProtoSet
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<ProtoCard> Cards { get; set; }
}
