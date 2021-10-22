namespace Archetype.Core
{
    public interface IGameState
    {
        IGamePiece GetGamePiece(long id);
        
        IPlayer Player { get; }
        IBoard Map { get; }
        
        ICardPool CardPool { get; }
    }
}
