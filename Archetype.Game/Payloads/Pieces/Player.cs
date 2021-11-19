
using System;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IPlayer : IGameAtom
    {
        string Name { get; }
        
        IDeck Deck { get; }
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }

        int Mill(int strength);
    }
    
    public class Player : Atom, IPlayer
    {
        public Player()
        {
            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
        }

        public string Name { get; }
        public IDeck Deck { get; }
        public IHand Hand { get; }
        public IDiscardPile DiscardPile { get; }
        public int Resources { get; set; }
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
    }
}
