using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IGameState
    {
        IPlayer Player { get; }
        IMap Map { get; }
        IHistoryReader HistoryReader { get; }
    }
    
    public class GameState : IGameState
    {
        public GameState(IMap map, IPlayer player, IHistoryReader historyReader)
        {
            Map = map;
            Player = player;
            HistoryReader = historyReader;
        }

        public IPlayer Player { get; }
        public IMap Map { get; }
        public IHistoryReader HistoryReader { get; }
    }
}
