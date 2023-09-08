namespace Archetype.Framework;

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

public class Card : Atom
{
    public Zone CurrentZone { get; set; }
    public ProtoCard Proto { get; set; }
    public object Modifiers { get; set; } // TODO: Define this
    public object RulesText { get; set; } // TODO: Define this
    
    // TODO: The card needs to have an API which uses the proto data in conjunction with the modifiers
}

