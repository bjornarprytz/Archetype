using Archetype.Core;
using Archetype.Rules.Proto;
using Archetype.Rules.State;

namespace Archetype.Rules;

public class Event
{
    public bool IsPrompt { get; set; } // TODO: Figure out a better mechanism. Type? Enum?
    public IEnumerable<Event> Children { get; set; }
}

public class ResolutionContext
{
    public IAtom Source { get; set; }
    public IReadOnlyList<Effect> Effects { get; set; }
    public IReadOnlyList<CostPayload> Costs { get; set; }
    
    public IDictionary<string, object> State { get; set; } // TODO: rename this
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