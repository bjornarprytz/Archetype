using Archetype.Core;

namespace Archetype.Rules.Proto;

public class TargetDescription
{
    public int Index { get; set; } // In the card target list
    
    public IReadOnlyDictionary<string, string> CharacteristicsMatch { get; set; }
    public string Description { get; set; }
    public bool IsOptional { get; set; }
}

public class OperandDescription
{
    public KeywordOperandType Type { get; set; }
    
    public bool IsComputed { get; set; }
    public object Value { get; set; } // If not computed
    public string ComputedPropertyKey { get; set; } // If computed, the value is cached with each card instance
}

public abstract class KeywordInstance
{
    public string Keyword { get; set; }
}

public class EffectInstance : KeywordInstance
{
    public IReadOnlyList<OperandDescription> Operands { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
}

public class ReactionInstance : KeywordInstance
{
    public EffectInstance Effect { get; set; }
}

public class FeatureInstance : KeywordInstance
{
    public int Stacks { get; set; }
}

public class AbilityInstance : KeywordInstance
{
    public IReadOnlyList<ConditionInstance> Conditions { get; set; }
    public IReadOnlyList<CostInstance> Costs { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }
}

public class ConditionInstance : KeywordInstance
{
    
}

public class CostInstance : KeywordInstance
{
    public int Amount { get; set; }
}

public class ComputedValueInstance : KeywordInstance
{
    public string Key { get; set; }
}
