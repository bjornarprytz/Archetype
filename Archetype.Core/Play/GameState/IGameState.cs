namespace Archetype.Core
{
    public interface IGameState
    {
        IPlayer Player { get; }
        IBoard Map { get; }
        
        ICardPool CardPool { get; }
    }
}
