namespace Archetype.Framework;

public class Definitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}

public abstract class Atom
{
    public Guid Id { get; set; }
}

public class GameState
{
    
}

public class Card : Atom
{
    public ProtoCard Proto { get; set; }
}