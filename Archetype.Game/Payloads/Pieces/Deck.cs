using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDeck : IZone<ICard>
    {
        ICard Draw();
        void Shuffle();
        void PutCardOnTop(ICard card);
        void PutCardOnBottom(ICard card);
    }
    
    public class Deck : Zone<ICard>, IDeck
    {
        private readonly Stack<ICard> _cards = new();

        public Deck(IGameAtom owner) : base(owner) { }

        public ICard Draw()
        {
            var card = _cards.Pop();
            
            RemovePiece(card);

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
            
            AddPiece(newCard);
        }

        public void PutCardOnBottom(ICard newCard)
        {
            var newOrder = _cards.Prepend(newCard).ToList();
            
            _cards.Clear();
            
            foreach (var card in newOrder)
            {
                _cards.Push(card);
            }
            
            AddPiece(newCard);
        }
    }
}
