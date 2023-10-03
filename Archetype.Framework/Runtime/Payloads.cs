using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public interface IEvent
{
    public IEvent Parent { get; set; }
    public IReadOnlyList<IEvent> Children { get; set; }
}

public abstract record EventBase : IEvent
{
    public IEvent? Parent { get; set; }
    public IReadOnlyList<IEvent> Children { get; set; } = new List<IEvent>();
}

public record NonEvent : EventBase;

public record ActionBlockEvent
    (IAtom Source, IReadOnlyList<IAtom> Targets, IReadOnlyDictionary<CostType, PaymentPayload> Payment) : EventBase
{
    public ActionBlockEvent(IResolutionContext context) : this(context.Source, context.Targets, context.Payments)
    {
        Children = context.Events.ToList();
    }
}


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

    public IList<IReadOnlyList<IAtom>> PromptResponses { get; }
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

    public IList<IReadOnlyList<IAtom>> PromptResponses { get; } = new List<IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
    public IDictionary<string, object> Memory { get; } = new Dictionary<string, object>();
}


public interface IEffectPayload
{
    IAtom Source { get; }
    string Keyword { get; }
    IReadOnlyList<object> Operands { get; }
    IReadOnlyList<IAtom> Targets { get; }
}
public record NestedEffectPayload(IAtom Source, string Keyword, IReadOnlyList<object> Operands, IReadOnlyList<IAtom> Targets) : EffectPayload(Source, Keyword, Operands, Targets) ;
public record EffectPayload(IAtom Source, string Keyword, IReadOnlyList<object> Operands, IReadOnlyList<IAtom> Targets) : IEffectPayload;


