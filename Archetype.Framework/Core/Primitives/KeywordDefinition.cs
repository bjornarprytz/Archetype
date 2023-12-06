using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Name { get; }
    string ReminderText { get; }
    IReadOnlyList<KeywordTargetDescription> Targets { get; }
    IReadOnlyList<IOperandDescription> Operands { get; }
    IKeywordInstance CreateInstance(IEnumerable<KeywordOperand> operands, IEnumerable<KeywordTarget> targets);
}
public abstract class KeywordDefinition : IKeywordDefinition
{
    public abstract string Name { get; } // ID
    public abstract string ReminderText { get; } // E.g. "Deal [X] damage to target unit or structure"
    protected virtual OperandDeclaration OperandDeclaration { get; } = new();
    protected virtual TargetDeclaration TargetDeclaration { get; } = new();
    public IReadOnlyList<KeywordTargetDescription> Targets => TargetDeclaration;
    public IReadOnlyList<IOperandDescription> Operands => OperandDeclaration;

    public IKeywordInstance CreateInstance(IEnumerable<KeywordOperand> operands, IEnumerable<KeywordTarget> targets)
    {
        var operandsList = operands.ToList();
        var targetsList = targets.ToList();

        if (!OperandDeclaration.Validate(operandsList))
        {
            throw new InvalidOperationException($"Invalid operands for keyword {Name}");
        }
        if (!TargetDeclaration.Validate(targetsList))
        {
            throw new InvalidOperationException($"Invalid targets for keyword {Name}");
        }
        
        return new KeywordInstance 
        {
            Keyword = Name,
            Operands = operandsList,
            Targets = targetsList,
        };
    }
}

public abstract class EffectPrimitiveDefinition : KeywordDefinition, IEffectPrimitiveDefinition
{
    public abstract IEvent Resolve(IResolutionContext context, EffectPayload effectPayload);
}

public interface IEffectPrimitiveDefinition : IKeywordDefinition
{
    public IEvent Resolve(IResolutionContext context, EffectPayload effectPayload);
}

public abstract class EffectCompositeDefinition : KeywordDefinition, IEffectCompositeDefinition
{
    public abstract IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload);
}

public interface IEffectCompositeDefinition : IKeywordDefinition
{
    IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload);
} 

public abstract class CharacteristicDefinition : KeywordDefinition { } 

public abstract class ConditionDefinition : KeywordDefinition
{
    public abstract bool Check(IResolutionContext context, IKeywordInstance keywordInstance); 
}

public abstract class CostDefinition : EffectCompositeDefinition
{
    public abstract CostType Type { get; }
    
    public abstract bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance);
}

public abstract class ComputedValueDefinition : KeywordDefinition
{
    public abstract int Compute(IResolutionContext context, IKeywordInstance keywordInstance);
}

