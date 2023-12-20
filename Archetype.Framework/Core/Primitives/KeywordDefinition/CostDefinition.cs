using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Core.Primitives;

public abstract class CostDefinition : EffectCompositeDefinition
{
    public abstract CostType Type { get; }
    
    public abstract bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance);
}