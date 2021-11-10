using System;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IGameState
    {
        bool IsPayerTurn { get; set; }
        
        IGameAtom GetGamePiece(Guid guid);
        
        IPlayer Player { get; }
        IMap Map { get; }
    }
    
    public class GameState : IGameState
    {
        public GameState(IMap map, IPlayer player)
        {
            Map = map;
            Player = player;
            IsPayerTurn = true;
        }
        
        public bool IsPayerTurn { get; set; }
        public IGameAtom GetGamePiece(Guid guid)
        {
            throw new NotImplementedException();
        }

        public IPlayer Player { get; }
        public IMap Map { get; }
    }
}
