using System;
using System.Linq;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IGameState
    {
        IGameAtom GetGamePiece(Guid guid);
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
        public IGameAtom GetGamePiece(Guid guid)
        {
            if (Player.Guid == guid)
            {
                return Player;
            }

            return Player.Hand.GetGamePiece(guid)
                   ?? Player.DiscardPile.GetGamePiece(guid)
                   ?? Player.Deck.GetGamePiece(guid)
                   ?? Map.Nodes.FirstOrDefault(node => node.Guid == guid)
                   ?? Map.Nodes.Select(node => node.GetGamePiece(guid)).FirstOrDefault()
                   ?? Map.Nodes.SelectMany(node => node.Contents.Select(unit => unit.Deck.GetGamePiece(guid))).FirstOrDefault();
        }

        public IPlayer Player { get; }
        public IMap Map { get; }
        public IHistoryReader HistoryReader { get; }
    }
}
