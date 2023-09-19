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
    public IReadOnlyList<IEvent> Children { get; set; }
}

public record PromptEvent(IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks) : EventBase;

public record EffectEvent(Effect EffectPayload) : EventBase;

public record ActionBlockEvent
    (IAtom Source, IReadOnlyList<IAtom> Targets, IReadOnlyList<CostPayload> Payment) : EventBase
{
    public ActionBlockEvent(IResolutionContext context) : this(context.Source, context.Targets, context.Costs)
    {
        Children = context.Events.ToList();
    }
}




public interface IResolutionContext
{
    public IGameState GameState { get; }
    public IAtom Source { get; }
    public IReadOnlyList<Effect> Effects { get; }
    public IReadOnlyList<CostPayload> Costs { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    
    public IList<IReadOnlyList<IAtom>> PromptResponses { get; }
    public IList<IEvent> Events { get; }
    public IDictionary<string, object> Memory { get; } 
}

public class ResolutionContext : IResolutionContext
{
    public required IGameState GameState { get; init; }
    public required IAtom Source { get; init; }
    public required IReadOnlyList<Effect> Effects { get; init; }
    public required IReadOnlyList<CostPayload> Costs { get; init; }
    public required IReadOnlyList<IAtom> Targets { get; init; }

    public IList<IReadOnlyList<IAtom>> PromptResponses { get; } = new List<IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
    public IDictionary<string, object> Memory { get; } = new Dictionary<string, object>();
}

public class Effect
{
    public required IAtom Source { get; init; }
    public required string Keyword { get; init; }
    public required IReadOnlyList<object> Operands { get; init; }
    
    public required IReadOnlyDictionary<int, IAtom> Targets { get; init; }
}
