using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Name { get; }
    string ReminderText { get; }
    IReadOnlyList<IOperandDescription> Operands { get; }
    IKeywordInstance CreateInstance(IEnumerable<KeywordOperand> operands);
}
public abstract class KeywordDefinition : IKeywordDefinition
{
    public abstract string Name { get; } // ID
    public abstract string ReminderText { get; } // E.g. "Deal [X] damage to target unit or structure"
    protected virtual OperandDeclaration OperandDeclaration { get; } = new();
    public IReadOnlyList<IOperandDescription> Operands => OperandDeclaration;

    public IKeywordInstance CreateInstance(IEnumerable<KeywordOperand> operands)
    {
        var operandsList = operands.ToList();

        if (!OperandDeclaration.Validate(operandsList))
        {
            throw new InvalidOperationException($"Invalid operands for keyword {Name}");
        }
        
        return new KeywordInstance 
        {
            Keyword = Name,
            Operands = operandsList,
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

