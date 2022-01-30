using System;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IPlayer : IGameAtom, IPlayerFront
    {
        new IStructure HeadQuarters { get; }
        
        new IDeck Deck { get; }
        new IHand Hand { get; }
        
        IEffectResult<IPlayer, int> Draw(int strength);

        IEffectResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure);
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

        public IEffectResult<IPlayer, int> Draw(int strength)
        {
            var actualStrength = Math.Clamp(strength, 0, Deck.NumberOfCards);
            
            for (var i=0; i < actualStrength; i++)
            {
                var card = Deck.PopCard();
                card.MoveTo(Hand);
            }

            return ResultFactory.Create(this, actualStrength);
        }

        public IEffectResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure)
        {
            HeadQuarters = structure;
            
            return ResultFactory.Create(this, structure);
        }
    }
}
