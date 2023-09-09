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

public class Card : Atom
{
    public Zone CurrentZone { get; set; }
    public ProtoCard Proto { get; set; }
    public object Modifiers { get; set; } // TODO: Define this
    public object RulesText { get; set; } // TODO: Define this
    
    private IDictionary<string, int> ComputedIntegers { get; set; } // Use this to create rules text
    private IDictionary<string, string> ComputedStrings { get; set; } // Use this to create rules text
    
    public IEnumerable<Effect> CreateEffects(PlayCardArgs args)
    {
        
    }
}

public class Ability
{
    public Card Source { get; set; }
    public ProtoAbility Proto { get; set; }
}