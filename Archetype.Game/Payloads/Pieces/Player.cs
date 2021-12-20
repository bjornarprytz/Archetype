using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Player")]
    public interface IPlayer : IGameAtom
    {
        int MaxHandSize { get; }
        int Resources { get; set; }
        
        IStructure HeadQuarters { get; }
        
        [Target("Player's deck")]
        IDeck Deck { get; }
        [Target("Player's hand")]
        IHand Hand { get; }
        
        int Draw(int strength); // TODO: Return a result like the other game actions
    }
    
    public class Player : Atom, IPlayer
    {
        private readonly IPlayerData _protoData;

        public Player(IPlayerData protoData)
        {
            _protoData = protoData;

            Resources = _protoData.StartingResources;
            
            Deck = new Deck(this);
            Hand = new Hand(this);
        }
        public int MaxHandSize => _protoData.MaxHandSize;
        public int Resources { get; set; }
        public IStructure HeadQuarters { get; private set; }
        public IDeck Deck { get; }
        public IHand Hand { get; }

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
