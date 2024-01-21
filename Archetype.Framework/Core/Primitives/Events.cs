using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IEvent
{
    public IAtom Source { get; }
    public IEvent Parent { get; set; }
    public IList<IEvent> Children { get; }
}

public interface IActionBlockEvent : IEvent
{
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; }
    
    public IReadOnlyList<int> ComputedValues { get; }
    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; } 
}

public interface IEffectEvent : IEvent
{
    public EffectPayload EffectPayload { get; }
}

public abstract record EventBase(IAtom Source) : IEvent
{
    public IEvent? Parent { get; set; }
    public IList<IEvent> Children { get; set; } = new List<IEvent>();
}

public record EffectEvent(EffectPayload EffectPayload) : EventBase(EffectPayload.Source) , IEffectEvent { } 

public record ActionBlockEvent
(
    IAtom Source, 
    IReadOnlyList<IAtom> Targets, 
    IReadOnlyDictionary<CostType, PaymentPayload> Payments,
    IReadOnlyList<int> ComputedValues,
    IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses
) : EventBase(Source), IActionBlockEvent
{
    public ActionBlockEvent(IResolutionContext context) : this(context.Source, context.Targets, context.Payments, context.ComputedValues, context.PromptResponses)
    {
        Children = context.Events.ToList();
    }
}
