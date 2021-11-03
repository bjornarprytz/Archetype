using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface IEffectResolutionContext<out TTarget> : IEffectResolutionContext
        where TTarget : IGamePiece
    {
        TTarget Target { get; }
    }

    public interface IEffectResolutionContext
    {
        ICardResolutionContext CardResolutionContext { get; }

        IGameState GameState => CardResolutionContext.GameState;
    }
}