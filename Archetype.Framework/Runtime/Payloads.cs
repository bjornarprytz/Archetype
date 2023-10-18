using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public interface IEvent
{
    public IEvent Parent { get; set; }
    public IList<IEvent> Children { get; }
}

public interface IActionBlockEvent : IEvent
{
    public IAtom Source { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; }
    
    public IReadOnlyList<int> ComputedValues { get; }
    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; } 
}

public abstract record EventBase : IEvent
{
    public IEvent? Parent { get; set; }
    public IList<IEvent> Children { get; set; } = new List<IEvent>();
}

public record NonEvent : EventBase;

public record ActionBlockEvent
    (
        IAtom Source, 
        IReadOnlyList<IAtom> Targets, 
        IReadOnlyDictionary<CostType, PaymentPayload> Payments,
        IReadOnlyList<int> ComputedValues,
        IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses
        ) : EventBase, IActionBlockEvent
{
    public ActionBlockEvent(IResolutionContext context) : this(context.Source, context.Targets, context.Payments, context.ComputedValues, context.PromptResponses)
    {
        Children = context.Events.ToList();
    }
}

public interface IKeywordFrame
{
    public IEvent Event { get; }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record KeywordFrame(IEvent Event, IReadOnlyList<IKeywordInstance> Effects) : IKeywordFrame;


public interface IResolutionFrame
{
    IResolutionContext Context { get; }
    IReadOnlyList<IKeywordInstance> Costs { get; }
    IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record ResolutionFrame(IResolutionContext Context, IReadOnlyList<IKeywordInstance> Costs,
    IReadOnlyList<IKeywordInstance> Effects) : IResolutionFrame;

public interface IResolutionContext
{
    public IMetaGameState MetaGameState { get; }
    public IGameState GameState { get; }
    public IAtom Source { get; }
    public IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyList<int> ComputedValues { get; }

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; }
    public IList<IEvent> Events { get; }
    public IDictionary<string, object> Memory { get; } // TODO: Evaluate if this is needed
}

public class ResolutionContext : IResolutionContext
{
    public required IMetaGameState MetaGameState { get; init; }
    public required IGameState GameState { get; init; }
    public required IAtom Source { get; init; }
    public required IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; init; }
    public required IReadOnlyList<IAtom> Targets { get; init; }
    public required IReadOnlyList<int> ComputedValues { get; init; }

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; } = new Dictionary<Guid, IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
    public IDictionary<string, object> Memory { get; } = new Dictionary<string, object>();
}

public record EffectPayload(Guid Id, IAtom Source, string Keyword, IReadOnlyList<object?> Operands, IReadOnlyList<IAtom> Targets);


