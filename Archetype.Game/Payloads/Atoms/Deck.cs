using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IDeck : IZone<ICard>, IDeckFront
    {
        new IObservable<IAtomMutation<IDeck>> OnMutation { get; }
        ICard PopCard();
        void Shuffle();
        void PutCardOnTop(ICard card);
        void PutCardOnBottom(ICard card);
    }

    internal class Deck : Zone<ICard>, IDeck
    {
        private readonly Stack<ICard> _cards = new();
        private readonly Subject<IAtomMutation<IDeck>> _mutation = new();

        public Deck(IGameAtom owner) : base(owner) {}

        public override IObservable<IAtomMutation> OnMutation => _mutation;
        IObservable<IAtomMutation<IDeck>> IDeck.OnMutation => _mutation;

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
