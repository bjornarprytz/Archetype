using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDeckFront : IZoneFront
    {
        int NumberOfCards { get; }
    }
    
    [Target("Deck")]
    internal interface IDeck : IZone<ICard>, IDeckFront
    {
        ICard PopCard();
        void Shuffle();
        void PutCardOnTop(ICard card);
        void PutCardOnBottom(ICard card);
    }
    
    internal class Deck : Zone<ICard>, IDeck
    {
        private readonly Stack<ICard> _cards = new();

        public Deck(IGameAtom owner) : base(owner) { }

        public ICard PopCard()
        {
            var card = _cards.Pop();

            return card;
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

            newCard.MoveTo(this);
        }

        public void PutCardOnBottom(ICard newCard)
        {
            var newOrder = _cards.Prepend(newCard).ToList();
            
            _cards.Clear();
            
            foreach (var card in newOrder)
            {
                _cards.Push(card);
            }

            newCard.MoveTo(this);
        }

        public int NumberOfCards => Contents.Count();
    }
}
