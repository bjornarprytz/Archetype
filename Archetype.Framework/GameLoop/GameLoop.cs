using Archetype.Framework.Events;

namespace Archetype.Framework.GameLoop;

public interface IGameLoop
{
    IScope GetCurrentScope();
}

public class GameLoop(Game game) : IGameLoop
{
    public IScope GetCurrentScope()
    {
        throw new NotImplementedException();
    }
}

