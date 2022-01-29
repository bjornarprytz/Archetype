using System;
using Archetype.Game.Payloads.Atoms;
using Archetype.View;
using Archetype.View.Atoms;
using Archetype.View.Events;
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
        public IMap Map { get; }
        IPlayerFront IGameStateFront.Player => Player;
        IMapFront IGameStateFront.Map => Map;

    }
}
