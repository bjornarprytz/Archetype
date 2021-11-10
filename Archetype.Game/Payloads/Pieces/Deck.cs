using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDeck : IZone<ICard>
    {
        ICard Draw();
        void Shuffle();
        void PutCardOnTop(ICard card);
        void PutCardOnBottom(ICard card);
    }
    
    public class Deck : GamePiece, IDeck
    {
        private readonly Stack<ICard> _cards = new();

        public Deck(IGamePiece owner) : base(owner)
        {
        }

        public IEnumerable<ICard> Contents => _cards;
        
        public ICard Draw()
        {
            return _cards.Pop();
        }

        public void Shuffle()
        {
            var shuffledCards = _cards.Shuffle();
            
            _cards.Clear();

            foreach (var card in shuffledCards)
            {
                _cards.Push(card);
            }
        }

        public void PutCardOnTop(ICard newCard)
        {
            _cards.Push(newCard);
        }

        public void PutCardOnBottom(ICard newCard)
        {
            var newOrder = _cards.Prepend(newCard).ToList();
            
            _cards.Clear();
            
            foreach (var card in newOrder)
            {
                _cards.Push(card);
            }
        }
    }
}
