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
    public interface IPlayerFront
    {
        int MaxHandSize { get; }
        int Resources { get; set; }
        IStructureFront HeadQuarters { get; }
        IDeckFront Deck { get; }
        IHandFront Hand { get; }
        
    }
    
    [Target("Player")]
    internal interface IPlayer : IGameAtom, IPlayerFront
    {
        new IStructure HeadQuarters { get; }
        
        [Target("Player's deck")]
        new IDeck Deck { get; }
        [Target("Player's hand")]
        new IHand Hand { get; }
        
        int Draw(int strength); // TODO: Return a result like the other game actions

        IResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure);
    }
    
    internal class Player : Atom, IPlayer
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
        IDeckFront IPlayerFront.Deck => Deck;
        IHandFront IPlayerFront.Hand => Hand;
        IStructureFront IPlayerFront.HeadQuarters => HeadQuarters;

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

        public IResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure)
        {
            HeadQuarters = structure;
            
            return ResultFactory.Create(this, structure);
        }
    }
}
