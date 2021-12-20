using System;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Player")]
    public interface IPlayer : IGameAtom
    {
        int MaxHandSize { get; }
        int MinDeckSize { get; }
        int Resources { get; set; }
        
        IStructure HeadQuarters { get; }
        
        [Target("Player's deck")]
        IDeck Deck { get; }
        [Target("Player's hand")]
        IHand Hand { get; }

        void SetHeadquarters(IStructure newHq);
        
        int Draw(int strength); // TODO: Return a result like the other game actions
    }
    
    public class Player : Atom, IPlayer
    {
        public Player()
        {
            Deck = new Deck(this);
            Hand = new Hand(this);
        }
        public int MaxHandSize { get; } = 2;
        public int MinDeckSize { get; } = 4; // TODO: GEt this from somewhere else
        public int Resources { get; set; } = 100;
        public IStructure HeadQuarters { get; private set; }
        public IDeck Deck { get; }
        public IHand Hand { get; }

        public void SetHeadquarters(IStructure newHq)
        {
            HeadQuarters = newHq;
        }

        public int Draw(int strength)
        {
            if (strength < 0)
                throw new ArgumentException($"Cannot draw a negative number ({strength}) of cards");

            var actualStrength = Math.Min(strength, Deck.Contents.Count());
            
            for (var i=0; i < actualStrength; i++)
            {
                var card = Deck.PopCard();
                card.MoveTo(Hand);
            }

            return actualStrength;
        }
    }
}
