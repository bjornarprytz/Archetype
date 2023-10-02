using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public static class Declare
{
    public static ICompositeKeywordInstance CompositeKeyword(string keyword, IReadOnlyList<KeywordTarget> targets, IReadOnlyList<KeywordOperand> operands, IReadOnlyList<KeywordInstance> children)
    {
        return new CompositeKeywordInstance
        {
            Keyword = keyword,
            Targets = targets,
            Operands = operands,
            Children = children,
        };
    }

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
    
    public static IReadOnlyDictionary<string, KeywordInstance> Characteristics(params (string Keyword, string Value)[] characteristics)
    {
        return characteristics
            .Select(c => new KeywordInstance{Keyword = c.Keyword, Operands = Operands(Operand(c.Value)) })
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
        return new KeywordOperand { GetValue = _ => value };
    }
    
    public static KeywordOperand Operand(int value)
    {
        return new KeywordOperand { GetValue = _ => value };
    }
    
    public static KeywordOperand Operand(Func<IResolutionContext, object> getValueFunc)
    {
        return new KeywordOperand { GetValue = getValueFunc };
    }
}