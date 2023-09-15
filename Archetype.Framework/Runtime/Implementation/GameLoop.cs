namespace Archetype.Framework.Runtime.Implementation;

public class GameLoop : IGameLoop
{
    private readonly IActionQueue _actionQueue;

    public GameLoop(IActionQueue actionQueue)
    {
        _actionQueue = actionQueue;
    }

    public IGameAPI Advance()
    {
        // TODO: Implement this
        // resolve effects until it's empty or there's a prompt
        // if the player is allowed actions in this phase, return those
        // otherwise, advance to the next phase, enqueueing the steps (which should be ActionBlocks)
        // if we're out of phases, restart the loop (turn)
        
        // Check victory conditions now and then (when?)

        throw new NotImplementedException();
    }
}