using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IGameStateFront
    {
        IPlayerFront Player { get; }
        IMapFront Map { get; }
    }
    
    internal interface IGameState : IGameStateFront
    {
        new IPlayer Player { get; }
        new IMap Map { get; }
        IHistoryReader HistoryReader { get; }
    }
    
    internal class GameState : IGameState
    {
        public GameState(IMap map, IPlayer player, IHistoryReader historyReader)
        {
            Map = map;
            Player = player;
            HistoryReader = historyReader;
        }

        public IPlayer Player { get; }
        IMapFront IGameStateFront.Map => Map;
        IPlayerFront IGameStateFront.Player => Player;

        public IMap Map { get; }
        public IHistoryReader HistoryReader { get; }
    }
}
