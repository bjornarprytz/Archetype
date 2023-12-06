using System.Collections;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;


public record OperandDescription<T>() : IOperandDescription
{
    public bool IsOptional { get; init; }
    public Type Type => typeof(T);
}

public interface IOperandDescription
{
    public bool IsOptional { get; init; }
    public Type Type { get; }
}


public class OperandDeclaration : IReadOnlyList<IOperandDescription>
{
    protected IReadOnlyList<IOperandDescription> Operands { get; init; } = new List<IOperandDescription>();
    public virtual bool Validate(IReadOnlyList<KeywordOperand> operands) => operands.Count == 0;
    public IEnumerator<IOperandDescription> GetEnumerator()
    {
        return Operands.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Operands).GetEnumerator();
    }

    public int Count => Operands.Count;

    public IOperandDescription this[int index] => Operands[index];
}
public class OperandDeclaration<T0> : OperandDeclaration
{
    public OperandDeclaration(bool isOptional = false)
    {
        Operands = new List<IOperandDescription>
        {
            new OperandDescription<T0> {IsOptional = isOptional},
        };
    }

    public override bool Validate(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Count == 1 && operands[0].Type == typeof(T0);
    }

    public T0 Unpack(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0>();
    }
    
    public T0 Unpack(IKeywordInstance keywordInstance, IResolutionContext? context=null)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(context)).Deconstruct<T0>();
    }
    
    public T0 Unpack(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0>();
    }
}

public class OperandDeclaration<T0, T1> : OperandDeclaration
{
    public OperandDeclaration(int nOptional = 0)
    {
        Operands = new List<IOperandDescription>
        {
            new OperandDescription<T0> {IsOptional = nOptional >= 2},
            new OperandDescription<T1> {IsOptional = nOptional >= 1},
        };
    }

    public override bool Validate(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Count == 2 && operands[0].Type == typeof(T0) && operands[1].Type == typeof(T1);
    }

    public (T0, T1) Unpack(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1>();
    }
    
    public (T0, T1) Unpack(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1>();
    }
    
    public (T0, T1) Unpack(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1>();
    }
}

public class OperandDeclaration<T0, T1, T2> : OperandDeclaration
{
    public OperandDeclaration(int nOptional = 0)
    {
        Operands = new List<IOperandDescription>
        {
            new OperandDescription<T0> {IsOptional = nOptional >= 3},
            new OperandDescription<T1> {IsOptional = nOptional >= 2},
            new OperandDescription<T2> {IsOptional = nOptional >= 1},
        };
    }

    public override bool Validate(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Count == 3 && operands[0].Type == typeof(T0) && operands[1].Type == typeof(T1) && operands[2].Type == typeof(T2);
    }

    public (T0, T1, T2) Unpack(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1, T2>();
    }
    
    public (T0, T1, T2) Unpack(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2>();
    }
    
    public (T0, T1, T2) Unpack(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2>();
    }
}

public class OperandDeclaration<T0, T1, T2, T3> : OperandDeclaration
{
    public OperandDeclaration(int nOptional = 0)
    {
        Operands = new List<IOperandDescription>
        {
            new OperandDescription<T0> {IsOptional = nOptional >= 4},
            new OperandDescription<T1> {IsOptional = nOptional >= 3},
            new OperandDescription<T2> {IsOptional = nOptional >= 2},
            new OperandDescription<T3> {IsOptional = nOptional >= 1},
        };
    }

    public override bool Validate(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Count == 4 && operands[0].Type == typeof(T0) && operands[1].Type == typeof(T1) && operands[2].Type == typeof(T2) && operands[3].Type == typeof(T3);
    }

    public (T0, T1, T2, T3) Unpack(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1, T2, T3>();
    }
    
    public (T0, T1, T2, T3) Unpack(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3>();
    }
    
    public (T0, T1, T2, T3) Unpack(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3>();
    }
}

public class OperandDeclaration<T0, T1, T2, T3, T4> : OperandDeclaration
{
    public OperandDeclaration(int nOptional = 0)
    {
        Operands = new List<IOperandDescription>
        {
            new OperandDescription<T0> {IsOptional = nOptional >= 5},
            new OperandDescription<T1> {IsOptional = nOptional >= 4},
            new OperandDescription<T2> {IsOptional = nOptional >= 3},
            new OperandDescription<T3> {IsOptional = nOptional >= 2},
            new OperandDescription<T4> {IsOptional = nOptional >= 1},
        };
    }

    public override bool Validate(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Count == 5 && operands[0].Type == typeof(T0) && operands[1].Type == typeof(T1) && operands[2].Type == typeof(T2) && operands[3].Type == typeof(T3) && operands[4].Type == typeof(T4);
    }

    public (T0, T1, T2, T3, T4) Unpack(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1, T2, T3, T4>();
    }
    
    public (T0, T1, T2, T3, T4) Unpack(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3, T4>();
    }
    
    public (T0, T1, T2, T3, T4) Unpack(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3, T4>();
    }
}
