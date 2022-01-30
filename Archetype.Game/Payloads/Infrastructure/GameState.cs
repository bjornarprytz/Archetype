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
    }
    
    internal class GameState : IGameState
    {
        public GameState(IMap map, IPlayer player)
        {
            Map = map;
            Player = player;
        }

        public IPlayer Player { get; }
        IMapFront IGameStateFront.Map => Map;
        IPlayerFront IGameStateFront.Player => Player;

        public IMap Map { get; }
    }
}
