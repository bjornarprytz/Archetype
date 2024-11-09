using Archetype.Framework.Events;

namespace Archetype.Framework.GameLoop;

public interface IGameLoop
{
    Scope GetCurrentScope();
}

public class GameLoop
{
    // TODO: Figure out what this should look like in order to support game actions, rules and events
    public Scope GetCurrentScope()
    {
        throw new NotImplementedException();
    }
}

public enum ScopeLevel
{
    Game,
    Turn,
    Phase,
    Prompt
}

public record Scope(ScopeLevel Level, Scope? Parent, Scope[] Nested, Event[] Events);