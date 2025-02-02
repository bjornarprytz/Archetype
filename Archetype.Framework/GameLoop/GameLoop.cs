using Archetype.Framework.Events;

namespace Archetype.Framework.GameLoop;

public interface IGameLoop
{
    IScope GetCurrentScope();
    
    // TODO: Express somehow which actions are available
}

