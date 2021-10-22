using Archetype.Core;

namespace Archetype.Game
{
    public class GameState : IGameState
    {
        public GameState()
        {
            // TODO: Remove this
        }
        
        public GameState(
            IPlayer player, 
            IBoard map,
            ICardPool cardPool)
        {
            CardPool = cardPool;
            Player = player;
            Map = map;
        }
        
        public IPlayer Player { get; }
        public IBoard Map { get; }
        public ICardPool CardPool { get; }
    }
}