using Archetype.Core;
using Archetype.Runtime.State;

namespace Archetype.Rules;

public interface IEvent
{
    public bool IsPrompt { get; set; } // TODO: Figure out a better mechanism. Type? Enum?
    public IEnumerable<IEvent> Children { get; set; }
}

public class ResolutionContext
{
    public IAtom Source { get; set; }
    public IReadOnlyList<Effect> Effects { get; set; }
    public IReadOnlyList<CostPayload> Costs { get; set; }
    
    public IDictionary<string, object> State { get; set; } // TODO: rename this. It is essentially for storing state between effects, like answers to prompts 
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