
using System;
using System.Runtime.CompilerServices;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IPlayer : IGamePiece
    {
        IDeck Deck { get; }
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }

        int Mill(int strength);
    }
    
    public class Player : GamePiece, IPlayer
    {
        public Player() : base(null) // Would use "this" here, but cannot
        {
            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
        }

        public IDeck Deck { get; }
        public IHand Hand { get; }
        public IDiscardPile DiscardPile { get; }
        public int Resources { get; set; }
        public int Mill(int strength)
        {
            if (strength < 0)
                throw new ArgumentException("Cannot mill negative cards");
            
            for (var i=0; i < strength; i++)
            {
                var card = Deck.Draw();
                DiscardPile.Bury(card);
            }

            return strength;
        }
    }
}
