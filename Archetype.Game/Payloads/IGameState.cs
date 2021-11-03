using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads
{
    public interface IGameState
    {
        bool IsPayerTurn { get; set; }
        
        IGamePiece GetGamePiece(long id);
        
        IPlayer Player { get; }
        IMap Map { get; }
        
    }
}
