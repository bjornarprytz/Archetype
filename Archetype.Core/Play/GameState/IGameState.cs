namespace Archetype.Core
{
    public interface IGameState
    {
        bool IsPayerTurn { get; set; }
        
        IGamePiece GetGamePiece(long id);
        
        IPlayer Player { get; }
        IBoard Map { get; }
        
        ICardPool CardPool { get; }
    }
}
