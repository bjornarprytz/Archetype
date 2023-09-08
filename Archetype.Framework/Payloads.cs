namespace Archetype.Framework;

public class KeywordArgs
{
    public IEnumerable<int> Operands { get; set; }
}

public class EffectArgs
{
    public IEnumerable<Card> Targets { get; set; }
}

public class CardArgs
{
    public IEnumerable<Card> Targets { get; set; }
}

public class AbilityArgs
{
    public IEnumerable<Card> Targets { get; set; }
}

public class Event
{
    public IEnumerable<Event> Children { get; set; }
    
}

public class PlayCardPayload
{
    public Card Card { get; set; }
    public CardArgs CardArgs { get; set; }
    public IReadOnlyList<CostPayload> Payments { get; set; }
}

public class AbilityPayload
{
    public Card Card { get; set; }
    public int AbilityIndex { get; set; }
    public AbilityArgs AbilityArgs { get; set; }
    public IReadOnlyList<CostPayload> Payments { get; set; }
}

public class EffectPayload
{
    public string Keyword { get; set; }
    public KeywordArgs KeywordArgs { get; set; }
    public Card Source { get; set; }
    public EffectArgs EffectArgs { get; set; }
}

public class CostPayload
{
    public CostType Type { get; set; }
    public IEnumerable<Card> Payment { get; set; }
}