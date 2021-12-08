using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phase
{
    public interface IPhaseResolutionContext
    {
        IGameState GameState { get; }
        
        IResolution PartialResults { get; }
    }
}