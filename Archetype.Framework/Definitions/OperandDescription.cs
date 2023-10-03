using System.Collections;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.Framework.Definitions;


public record OperandDescription<T>() : OperandDescription(Helpers.GetParsedType<T>());

public interface IOperandDescription
{
    public bool IsOptional { get; init; }
    public KeywordOperandParsedType ParsedType { get; }
}

public abstract record OperandDescription(KeywordOperandParsedType ParsedType) : IOperandDescription
{
    public bool IsOptional { get; init; }
}


public class OperandDeclaration : IReadOnlyList<IOperandDescription>
{
    protected IReadOnlyList<IOperandDescription> Operands { get; init; } = new List<IOperandDescription>();
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

    public T0 UnpackOperands(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0>();
    }
    
    public T0 UnpackOperands(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0>();
    }
    
    public T0 UnpackOperands(IReadOnlyList<KeywordOperand> operands)
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
    
    public (T0, T1) UnpackOperands(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1>();
    }
    
    public (T0, T1) UnpackOperands(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1>();
    }
    
    public (T0, T1) UnpackOperands(IReadOnlyList<KeywordOperand> operands)
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
    
    public (T0, T1, T2) UnpackOperands(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1, T2>();
    }
    
    public (T0, T1, T2) UnpackOperands(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2>();
    }
    
    public (T0, T1, T2) UnpackOperands(IReadOnlyList<KeywordOperand> operands)
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
    
    public (T0, T1, T2, T3) UnpackOperands(EffectPayload effectPayload)
    {
        return effectPayload.Operands.Deconstruct<T0, T1, T2, T3>();
    }
    
    public (T0, T1, T2, T3) UnpackOperands(IKeywordInstance keywordInstance)
    {
        return keywordInstance.Operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3>();
    }
    
    public (T0, T1, T2, T3) UnpackOperands(IReadOnlyList<KeywordOperand> operands)
    {
        return operands.Select(operand => operand.GetValue(null)).Deconstruct<T0, T1, T2, T3>();
    }
}
