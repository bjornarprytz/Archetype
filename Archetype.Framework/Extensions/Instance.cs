using System.Collections.Immutable;
using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Framework.Extensions;

public static class Instance
{
    /*
     * 
    public static IKeywordInstance Bind(Delegate resolveFunc, params IKeywordOperand[] operands)
    {
        resolveFunc.RequireEffectAttribute();
        
        var parameterList = resolveFunc.Method.GetParameters();
        
        if (parameterList.Length  != operands.Length)
            throw new InvalidOperationException($"Method {resolveFunc.Method.Name} has {parameterList.Length} parameters but {operands.Length} operands were provided");
        
        for (var i = 0; i < parameterList.Length; i++)
        {
            var parameter = parameterList[i];
            var operand = operands[i];
            
            if (parameter.ParameterType != operand.Type)
                throw new InvalidOperationException($"Method {resolveFunc.Method.Name} parameter {parameter.Name} is of type {parameter.ParameterType.Name} but operand {i} is of type {operand.Type.Name}");
        }
        
        return new KeywordInstance(resolveFunc.Method.Name, operands);
    }
     */
    
    public static IKeywordInstance Bind(Func<IResolutionContext, IEffectResult> resolveFunc)
    {
        resolveFunc.RequireEffectAttribute();

        return new KeywordInstance(resolveFunc.Method.Name);
    }
    
    public static IKeywordInstance BindArgs<T1>(Func<IResolutionContext, T1, IEffectResult> resolveFunc, T1 arg)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, arg.ToOperand());
    }
    
    public static IKeywordInstance Bind<T1>(Func<IResolutionContext, T1, IEffectResult> resolveFunc, IKeywordOperand<T1> operand)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, operand);
    }
    
    public static IKeywordInstance BindArgs<T1, T2>(Func<IResolutionContext, T1, T2, IEffectResult> resolveFunc, T1 arg1, T2 arg2)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance(resolveFunc.Method.Name, arg1.ToOperand(), arg2.ToOperand());
    }
    
    public static IKeywordInstance Bind<T1, T2>(Func<IResolutionContext, T1, T2, IEffectResult> resolveFunc, IKeywordOperand<T1> operand1, IKeywordOperand<T2> operand2)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance(resolveFunc.Method.Name, operand1, operand2);
    }
    
    public static IKeywordInstance BindArgs<T1, T2, T3>(Func<IResolutionContext, T1, T2, T3, IEffectResult> resolveFunc, T1 arg1, T2 arg2, T3 arg3)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, arg1.ToOperand(), arg2.ToOperand(), arg3.ToOperand());
    }
    
    public static IKeywordInstance Bind<T1, T2, T3>(Func<IResolutionContext, T1, T2, T3, IEffectResult> resolveFunc, IKeywordOperand<T1> operand1, IKeywordOperand<T2> operand2, IKeywordOperand<T3> operand3)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, operand1, operand2, operand3);
    }
    
    public static IKeywordInstance BindArgs<T1, T2, T3, T4>(Func<IResolutionContext, T1, T2, T3, T4, IEffectResult> resolveFunc, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, arg1.ToOperand(), arg2.ToOperand(), arg3.ToOperand(), arg4.ToOperand());
    }
    
    public static IKeywordInstance Bind<T1, T2, T3, T4>(Func<IResolutionContext, T1, T2, T3, T4, IEffectResult> resolveFunc, IKeywordOperand<T1> operand1, IKeywordOperand<T2> operand2, IKeywordOperand<T3> operand3, IKeywordOperand<T4> operand4)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, operand1, operand2, operand3, operand4);
    }
    
    public static IKeywordInstance BindArgs<T1, T2, T3, T4, T5>(Func<IResolutionContext, T1, T2, T3, T4, T5, IEffectResult> resolveFunc, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, arg1.ToOperand(), arg2.ToOperand(), arg3.ToOperand(), arg4.ToOperand(), arg5.ToOperand());
    }
    
    public static IKeywordInstance Bind<T1, T2, T3, T4, T5>(Func<IResolutionContext, T1, T2, T3, T4, T5, IEffectResult> resolveFunc, IKeywordOperand<T1> operand1, IKeywordOperand<T2> operand2, IKeywordOperand<T3> operand3, IKeywordOperand<T4> operand4, IKeywordOperand<T5> operand5)
    {
        resolveFunc.RequireEffectAttribute();
        
        return new KeywordInstance (resolveFunc.Method.Name, operand1, operand2, operand3, operand4, operand5);
    }
    
    private static void RequireEffectAttribute(this Delegate func)
    {
        var methodInfo = func.Method;
        
        if (!methodInfo.HasAttribute<EffectAttribute>())
            throw new InvalidOperationException($"Method {methodInfo.Name} is not annotated with {nameof(EffectAttribute)}");
    }
}