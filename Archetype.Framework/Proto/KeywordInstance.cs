using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Proto;

public record KeywordInstance
{
    public string Keyword { get; init; }
    public IReadOnlyList<KeywordOperand> Operands { get; init; }
    
    public IReadOnlyList<KeywordTarget> Targets { get; init; }
}

public record KeywordTarget(
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand
{
    public Func<IResolutionContext, object> GetValue { get; init; }
}

public record EffectInstance : KeywordInstance
{
}

public record ReactionInstance : KeywordInstance
{
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
}

public abstract record CharacteristicInstance : KeywordInstance
{
    public abstract object Value { get; }
    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}

public record CharacteristicInstance<T> : CharacteristicInstance
{
    public T TypedValue { get; }
    public override object Value => TypedValue;
}

public record ConditionInstance : KeywordInstance
{
    
}

public record CostInstance : KeywordInstance
{
    public int Amount { get; set; }
}

public record ComputedValueInstance : KeywordInstance
{
}
