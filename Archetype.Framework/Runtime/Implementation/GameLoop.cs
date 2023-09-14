namespace Archetype.Framework.Runtime.Implementation;

public class GameLoop : IGameLoop
{
    private readonly IEffectQueue _effectQueue;

    public GameLoop(IEffectQueue effectQueue)
    {
        _effectQueue = effectQueue;
    }
    
    public ActionResult Advance()
    {
        // resolve effects until it's empty or there's a prompt
        // if the player is allowed actions in this phase, return those
        // otherwise, advance to the next phase, enqueueing the steps (which should be ActionBlocks)
        // if we're out of phases, restart the loop (turn)
        
        // Check victory conditions now and then (when?)

        throw new NotImplementedException();
    }
}