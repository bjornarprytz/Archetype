using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public static class Declare
{
    public static IKeywordInstance KeywordInstance(string keyword, IReadOnlyList<KeywordTarget> targets, IReadOnlyList<KeywordOperand> operands)
    {
        return new KeywordInstance { Keyword = keyword, Operands = operands, Targets = targets };
    }
    public static IKeywordInstance KeywordInstance(string keyword, IReadOnlyList<KeywordTarget> targets)
    {
        return new KeywordInstance { Keyword = keyword, Targets = targets };
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
    
    public static IReadOnlyList<KeywordTarget> Targets(params KeywordTarget[] targets)
    {
        return targets.ToList();
    }
    
    public static KeywordTarget Target(IAtom atom)
    {
        return new KeywordTarget(_ => atom);
    }
    public static KeywordTarget Target(int index)
    {
        return new KeywordTarget(ctx => ctx.Targets[index]);
    }
    public static KeywordTarget TargetSource()
    {
        return new KeywordTarget(ctx => ctx.Source);
    }
    
    public static IReadOnlyList<KeywordOperand> Operands(params KeywordOperand[] operands)
    {
        return operands.ToList();
    }
    

    public static KeywordOperand Operand(string value)
    {
        return new KeywordOperand<string>(_ => value);
    }
    
    public static KeywordOperand Operand(int value)
    {
        return new KeywordOperand<int>(_ => value);
    }
    
    public static KeywordOperand Operand<T>(Func<IResolutionContext, T> getValueFunc)
    {
        return new KeywordOperand<T>(getValueFunc);
    }
}