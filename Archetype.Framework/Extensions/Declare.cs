using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class Declare
{
    
    
    public static IReadOnlyList<IKeywordInstance> KeywordInstances(params IKeywordInstance[] instances)
    {
        return instances.ToList();
    }
    
    public static IReadOnlyList<KeywordOperand> Operands(params KeywordOperand[] operands)
    {
        return operands.ToList();
    }
    
    public static KeywordOperand Operand<T>(T value)
    {
        return new KeywordOperand<T>(_ => value);
    }
    
    public static KeywordOperand<T> Operand<T>(Func<IResolutionContext?, T> getValueFunc)
    {
        return new KeywordOperand<T>(getValueFunc);
    }

    public static IReadOnlyList<PaymentPayload> Payments(params PaymentPayload[] payments)
    {
        return payments.ToList();
    }
}