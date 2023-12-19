using Archetype.Framework.Extensions;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Keyword { get; } // ID
    IReadOnlyList<IOperandDescription> Operands { get; }
    IKeywordInstance CreateInstance(params object[] operands);
}

public abstract class KeywordDefinition<TOperands> : KeywordDefinition
    where TOperands : OperandDeclaration, new()
{
    protected override OperandDeclaration OperandDeclaration { get; } = new TOperands();
}

public abstract class KeywordDefinition : IKeywordDefinition
{
    protected KeywordDefinition()
    {
        Keyword =  GetType().Name;
    }
    public string Keyword { get; }
    protected virtual OperandDeclaration OperandDeclaration { get; } = new();
    public IReadOnlyList<IOperandDescription> Operands => OperandDeclaration;

    public IKeywordInstance CreateInstance(params object[] operands)
    {
        var operandList = operands.Select(o => o.ToOperand()).ToList();
        
        if (!OperandDeclaration.Validate(operandList))
        {
            throw new InvalidOperationException($"Invalid operands for keyword ({(this.TryGetKeywordName(out var keywordName) ? keywordName : "Unknown")})");
        }
        
        return new KeywordInstance 
        {
            Keyword = Keyword,
            Operands = operandList,
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

public abstract class TargetSpecificationDefinition : KeywordDefinition
{
    protected sealed override OperandDeclaration<bool> OperandDeclaration { get; } = new();

    public bool IsOptional(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var isOptional = OperandDeclaration.Unpack(keywordInstance, context);
        return isOptional;
    }
    public abstract bool Filter(IAtom atom, IResolutionContext context, IKeywordInstance keywordInstance);
    public abstract IEnumerable<IAtom> GetAtoms(IResolutionContext context, IKeywordInstance keywordInstance);
}

