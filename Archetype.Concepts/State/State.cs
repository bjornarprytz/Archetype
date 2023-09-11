using Archetype.Rules.Definitions;
using Archetype.Rules.Proto;

namespace Archetype.Rules.State;

public class Definitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}

public abstract class Atom
{
    public Guid Id { get; set; }
}

public abstract class Zone : Atom
{
    public IEnumerable<Card> Cards { get; set; }
}

public class GameState
{
    
}

public interface IComputedPropertyCache
{
    object? GetComputedProperty(string key);
}

public class Card : Atom, IComputedPropertyCache
{
    public Zone CurrentZone { get; set; }
    public ProtoCard Proto { get; set; }
    public object Modifiers { get; set; } // TODO: Define this
    public object RulesText { get; set; } // TODO: Define this
    
    private IDictionary<string, int> ComputedIntegers { get; set; } // Use this to create rules text
    private IDictionary<string, string> ComputedStrings { get; set; } // Use this to create rules text

    public object? GetComputedProperty(string key)
    {
        if (ComputedIntegers.TryGetValue(key, out var value))
            return value;

        if (ComputedStrings.TryGetValue(key, out var stringValue))
            return stringValue;

        return null;
    }
}

public class Ability : IComputedPropertyCache
{
    public Card Source { get; set; }
    public AbilityInstance Proto { get; set; }
    
    private IDictionary<string, int> ComputedIntegers { get; set; } // Use this to create rules text
    private IDictionary<string, string> ComputedStrings { get; set; } // Use this to create rules text

    public object? GetComputedProperty(string key)
    {
        if (ComputedIntegers.TryGetValue(key, out var value))
            return value;

        if (ComputedStrings.TryGetValue(key, out var stringValue))
            return stringValue;

        return null;
    }
}