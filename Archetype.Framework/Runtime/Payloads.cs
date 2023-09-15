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
    public ActionBlockEvent(ResolutionContext context) : this(context.Source, context.Targets, context.Costs)
    {
        Children = context.Events.ToList();
    }
}

public class ResolutionContext
{
    public ResolutionContext()
    {
        PromptResponses = new List<IReadOnlyList<IAtom>>();
        Events = new List<IEvent>();
        State = new Dictionary<string, object>();
    }
    
    public IAtom Source { get; set; }
    public IReadOnlyList<Effect> Effects { get; set; }
    public IReadOnlyList<CostPayload> Costs { get; set; }
    public IReadOnlyList<IAtom> Targets { get; set; }
    
    public IList<IReadOnlyList<IAtom>> PromptResponses { get; set; }
    public IList<IEvent> Events { get; set; }
    public IDictionary<string, object> State { get; set; } // TODO: rename this. It is essentially for storing state between effects 
}

public class Effect
{
    public IAtom Source { get; set; }
    public string Keyword { get; set; }
    public IReadOnlyList<object> Operands { get; set; }
    
    public IReadOnlyDictionary<int, IAtom> Targets { get; set; }
}
