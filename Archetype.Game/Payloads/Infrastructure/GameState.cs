using Archetype.Game.Payloads.Atoms;
using Archetype.View;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Infrastructure
{
    

    public interface IGameState : IGameStateFront
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
