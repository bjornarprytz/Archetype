using Archetype.Core;
using Archetype.Rules.State;

namespace Archetype.Rules;

public class Event
{
    public IEnumerable<Event> Children { get; set; }
    
}

public class Effect
{
    public string Keyword { get; set; }
    public IReadOnlyList<int> KeywordOperands { get; set; }
    public Card Source { get; set; }
    public IReadOnlyList<Card> Targets { get; set; }
}

public class CostPayload
{
    public CostType Type { get; set; }
    public IReadOnlyList<Card> Payment { get; set; }
}