using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class Declare
{
    public static KeywordOperand ToOperand(this object value)
    {
        if (value is KeywordOperand operand)
        {
            return operand;
        }
        
        return new KeywordOperand(value.GetType(), _ => value);
    }
    
    public static IReadOnlyList<IKeywordInstance> KeywordInstances(params IKeywordInstance[] instances)
    {
        return instances.ToList();
    }
    
    public static IKeywordInstance KeywordInstance(string keyword, IReadOnlyList<KeywordOperand> operands)
    {
        return new KeywordInstance { Keyword = keyword, Operands = operands };
    }
    
    public static IKeywordInstance KeywordInstance(string keyword)
    {
        return new KeywordInstance { Keyword = keyword };
    }
    
    public static IReadOnlyDictionary<string, IKeywordInstance> Characteristics(params (string Keyword, string Value)[] characteristics)
    {
        return characteristics
            .Select(c => KeywordInstance(c.Keyword, Operands(Operand(c.Value))))
            .ToDictionary(kw => kw.Keyword, kw => kw);
    }
    public static KeywordOperand TargetSource()
    {
        return new KeywordOperand<IAtom>(ctx => ctx.Source);
    }
    
    public static IReadOnlyList<KeywordOperand> Operands(params KeywordOperand[] operands)
    {
        return operands.ToList();
    }
    
    public static KeywordOperand Operand<T>(T value)
    {
        return new KeywordOperand<T>(_ => value);
    }
    
    public static KeywordOperand Operand<T>(Func<IResolutionContext, T> getValueFunc)
    {
        return new KeywordOperand<T>(getValueFunc);
    }

    public static IReadOnlyList<PaymentPayload> Payments(params PaymentPayload[] payments)
    {
        return payments.ToList();
    }
}