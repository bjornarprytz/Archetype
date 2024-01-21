using System.Collections.Immutable;
using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Extensions;

public static class Instance
{
    public static IKeywordInstance Bind(Func<IResolutionContext, IEffectResult> resolveFunc)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance 
        {
            ResolveFuncName = resolveFunc.Method.Name,
            Operands = ImmutableList<IKeywordOperand>.Empty,
        };
    }
    
    public static IKeywordInstance Bind<T1>(Func<IResolutionContext, T1, IEffectResult> resolveFunc, T1 operand)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance 
        {
            ResolveFuncName = resolveFunc.Method.Name,
            Operands = new List<KeywordOperand> { operand.ToOperand() },
        };
    }
    
    public static IKeywordInstance Bind<T1, T2>(Func<IResolutionContext, T1, T2, IEffectResult> resolveFunc, T1 operand1, T2 operand2)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance 
        {
            ResolveFuncName = resolveFunc.Method.Name,
            Operands = new List<KeywordOperand> { operand1.ToOperand(), operand2.ToOperand() },
        };
    }
    
    public static IKeywordInstance Bind<T1, T2, T3>(Func<IResolutionContext, T1, T2, T3, IEffectResult> resolveFunc, T1 operand1, T2 operand2, T3 operand3)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance 
        {
            ResolveFuncName = resolveFunc.Method.Name,
            Operands = new List<KeywordOperand> { operand1.ToOperand(), operand2.ToOperand(), operand3.ToOperand() },
        };
    }
    
    public static IKeywordInstance Bind<T1, T2, T3, T4>(Func<IResolutionContext, T1, T2, T3, T4, IEffectResult> resolveFunc, T1 operand1, T2 operand2, T3 operand3, T4 operand4)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance 
        {
            ResolveFuncName = resolveFunc.Method.Name,
            Operands = new List<KeywordOperand> { operand1.ToOperand(), operand2.ToOperand(), operand3.ToOperand(), operand4.ToOperand() },
        };
    }
    
    private static void RequireEffectAttribute(this Delegate func)
    {
        var methodInfo = func.Method;
        
        if (!methodInfo.HasAttribute<EffectAttribute>())
            throw new InvalidOperationException($"Method {methodInfo.Name} is not annotated with {nameof(EffectAttribute)}");
    }
}