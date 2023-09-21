using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Proto;

public abstract class KeywordInstance
{
    public string Keyword { get; set; }
    public IReadOnlyList<KeywordOperand> Operands { get; set; }
}

public record KeywordTarget(
    Func<IAtom, bool> ValidateTarget,
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand
{
    public Func<IResolutionContext, object> GetValue { get; set; }
}

public class EffectInstance : KeywordInstance
{
    public IReadOnlyList<KeywordTarget> Targets { get; set; }
}

public class ReactionInstance : KeywordInstance
{
    public IReadOnlyList<TargetDescription> Targets { get; set; }
    public IReadOnlyList<ComputedValueInstance> ComputedValues { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
}

public class FeatureInstance : KeywordInstance
{
    public int Stacks { get; set; }
}

public class AbilityInstance : KeywordInstance
{
    public string Name { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
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
}
