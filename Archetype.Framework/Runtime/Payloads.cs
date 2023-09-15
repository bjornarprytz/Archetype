using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public interface IEvent
{
    public IEvent Parent { get; set; }
    public IReadOnlyList<IEvent> Children { get; set; }
}

public record PromptEvent(IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks) : IEvent
{
    public IEvent Parent { get; set; }
    public IReadOnlyList<IEvent> Children { get; set; }
}

public class ResolutionContext
{
    public IAtom Source { get; set; }
    public IReadOnlyList<Effect> Effects { get; set; }
    public IReadOnlyList<CostPayload> Costs { get; set; }
    
    public IList<IReadOnlyList<IAtom>> PromptResponses { get; set; }
    public IDictionary<string, object> State { get; set; } // TODO: rename this. It is essentially for storing state between effects 
}

public class Effect
{
    public IAtom Source { get; set; }
    public string Keyword { get; set; }
    public IReadOnlyList<object> Operands { get; set; }
    
    public IReadOnlyDictionary<int, IAtom> Targets { get; set; }
}

public class CostPayload
{
    public CostType Type { get; set; }
    public IReadOnlyList<ICard> Payment { get; set; }
}

