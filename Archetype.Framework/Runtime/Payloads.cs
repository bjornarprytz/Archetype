using System.Collections;
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


public record EffectEvent(Effect EffectPayload) : EventBase;

public record ActionBlockEvent
    (IAtom Source, IReadOnlyList<IAtom> Targets, IReadOnlyList<CostPayload> Payment) : EventBase
{
    public ActionBlockEvent(IResolutionContext context) : this(context.Source, context.Targets, context.Costs)
    {
        Children = context.Events.ToList();
    }
}


public interface IResolutionFrame
{
    IResolutionContext Context { get; }
    IReadOnlyList<EffectInstance> Effects { get; }
}

public record ResolutionFrame(IResolutionContext Context, IReadOnlyList<EffectInstance> Effects) : IResolutionFrame;

public interface IResolutionContext
{
    public IMetaGameState MetaGameState { get; }
    public IGameState GameState { get; }
    public IAtom Source { get; }
    public IReadOnlyList<CostPayload> Costs { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyList<object> ComputedValues { get; }

    public IList<IReadOnlyList<IAtom>> PromptResponses { get; }
    public IList<IEvent> Events { get; }
    public IDictionary<string, object> Memory { get; } 
}

public class ResolutionContext : IResolutionContext
{
    public required IMetaGameState MetaGameState { get; init; }
    public required IGameState GameState { get; init; }
    public required IAtom Source { get; init; }
    public required IReadOnlyList<CostPayload> Costs { get; init; }
    public required IReadOnlyList<IAtom> Targets { get; init; }
    public required IReadOnlyList<object> ComputedValues { get; init; }

    public IList<IReadOnlyList<IAtom>> PromptResponses { get; } = new List<IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
    public IDictionary<string, object> Memory { get; } = new Dictionary<string, object>();
}

public record Effect(IAtom Source, string Keyword, IReadOnlyList<object> Operands, IReadOnlyList<IAtom> Targets);


