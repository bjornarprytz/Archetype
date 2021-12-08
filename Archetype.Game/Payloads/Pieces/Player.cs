using System;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Player")]
    public interface IPlayer : IGameAtom
    {
        string Name { get; }
        int MaxHandSize { get; }
        int MinDeckSize { get; }
        int Resources { get; set; }
        
        IStructure HeadQuarters { get; }
        
        [Target("Player's deck")]
        IDeck Deck { get; }
        [Target("Player's hand")]
        IHand Hand { get; }
        [Target("Player's discard pile")]
        IDiscardPile DiscardPile { get; }

        int Mill(int strength);
        int Draw(int strength);
    }
    
    public class Player : Atom, IPlayer
    {
        public Player()
        {
            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
        }

        public string Name { get; } = "Test player";
        public int MaxHandSize { get; } = 2;
        public int MinDeckSize { get; } = 4;
        public int Resources { get; set; } = 100;
        public IStructure HeadQuarters { get; } // TODO: Figure out how to place this
        public IDeck Deck { get; }
        public IHand Hand { get; }
        public IDiscardPile DiscardPile { get; }
        public int Mill(int strength)
        {
            if (strength < 0)
                throw new ArgumentException($"Cannot mill a negative number ({strength}) of cards");
            
            for (var i=0; i < strength; i++)
            {
                var card = Deck.Draw();
                DiscardPile.Bury(card);
            }

            return strength;
        }

        public int Draw(int strength)
        {
            if (strength < 0)
                throw new ArgumentException($"Cannot draw a negative number ({strength}) of cards");

            var actualStrength = Math.Min(strength, Deck.Contents.Count());
            
            for (var i=0; i < actualStrength; i++)
            {
                var card = Deck.Draw();
                Hand.Add(card);
            }

            return actualStrength;
        }
    }
}
