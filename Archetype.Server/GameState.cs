using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Pieces;
using Archetype.Server.Extensions;

namespace Archetype.Server
{
    public class GameState : IGameState
    {
        private ICardPool _pool;
        private readonly IPlayer _player;
        private readonly IBoard _board;

        private Dictionary<long, IGamePiece> _gamePieces = new();

        public GameState(ICardPool pool)
        {
            _pool = pool;
            _player = new Player(new Hand(pool.Cards.First().CreateInstance()), new DiscardPile());
            _board = new Board();
            
        }
        
        public bool IsPayerTurn { get; set; }
        public IGamePiece GetGamePiece(long id)
        {
            return _gamePieces[id];
        }

        public IPlayer Player => _player;
        public IBoard Map => _board;
    }
    
    public class Player : IPlayer
    {
        private readonly IHand _hand;
        private readonly IDiscardPile _discardPile;

        public Player(IHand hand, IDiscardPile discardPile)
        {
            _hand = hand;
            _discardPile = discardPile;
        }
        
        public long OwnerId => Id;
        public int Resources { get; set; }
        public long Id { get; }
        public IDeck Deck { get; }
        public IHand Hand => _hand;
        public IDiscardPile DiscardPile => _discardPile;
    }
    
    public class Hand : IHand
    {
        private List<ICard> _cards = new ();
        public Hand(ICard card)
        {
            _cards.Add(card);
        }
        
        public long OwnerId { get; }
        public long Id { get; }
        public IEnumerable<ICard> Cards => _cards;
    }
    
    public class DiscardPile : IDiscardPile
    {
        public long OwnerId { get; }
        public long Id { get; }
        public IEnumerable<ICard> Cards { get; } = new List<ICard>();
    }
    
    public class Board : IBoard
    {
        public IEnumerable<IZone> Zones { get; } = new List<IZone>();
    }
}