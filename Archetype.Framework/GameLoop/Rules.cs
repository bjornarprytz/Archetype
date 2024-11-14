using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface IRules
{
    // How to initialize the game state
    // How to progress the game loop
    
    // TODO: Express the turn structure as turns phases and prompts, which actions can be taken, and how they progress the game loop
    
    public IGameState InitializeState(int seed);
}

public class Rules : IRules
{
    public IGameState InitializeState(int seed)
    {
        throw new NotImplementedException();
    }
}