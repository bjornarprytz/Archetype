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
    public Card Source { get; set; }
    public IReadOnlyList<Effect> Effects { get; set; }
    public IReadOnlyList<CostPayload> Costs { get; set; }
    
    public IDictionary<string, object> State { get; set; } // TODO: rename this
}

public interface IActionBlock
{
    public Card Source { get; }
    public IReadOnlyList<EffectInstance> Effects { get; }
    public IReadOnlyList<CostInstance> Costs { get; }

    public object? GetComputedValue(string key);
    void UpdateComputedValues(State.Definitions definitions, GameState gameState);
}

public class Effect
{
    public Card Source { get; set; }
    public string Keyword { get; set; }
    public IReadOnlyList<object> Operands { get; set; }
    
    public IReadOnlyDictionary<int, Card> Targets { get; set; }
}

public class CostPayload
{
    public CostType Type { get; set; }
    public IReadOnlyList<Card> Payment { get; set; }
}