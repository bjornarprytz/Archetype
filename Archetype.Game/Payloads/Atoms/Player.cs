using System;
using System.Linq;
using System.Reactive.Subjects;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IPlayer : IGameAtom, IPlayerFront
    {
        new IObservable<IAtomMutation<IPlayer>> OnMutation { get; }
        
        new IStructure HeadQuarters { get; }
        
        new IDeck Deck { get; }
        new IHand Hand { get; }
        
        [Keyword("Draw")]
        IResult<IPlayer, int> Draw(int strength);

        IResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure);
    }
    
    internal class Player : Atom, IPlayer
    {
        private readonly IPlayerData _protoData;
        private int _resources;
        private readonly Subject<IAtomMutation<IPlayer>> _mutation = new();

        public Player(IPlayerData protoData)
        {
            _protoData = protoData;

            Resources = _protoData.StartingResources;
            
            Deck = new Deck(this);
            Hand = new Hand(this);
        }
        public int MaxHandSize => _protoData.MaxHandSize;

        public int Resources
        {
            get => _resources;
            set
            {
                if (_resources == value)
                    return;
                
                _resources = value;
                _mutation.OnNext(new AtomMutation<IPlayer>(this));
            }
        }

        IObservable<IAtomMutation<IPlayer>> IPlayer.OnMutation => _mutation;
        public override IObservable<IAtomMutation> OnMutation => _mutation;

        public IStructure HeadQuarters { get; private set; }
        IDeckFront IPlayerFront.Deck => Deck;
        IHandFront IPlayerFront.Hand => Hand;
        IStructureFront IPlayerFront.HeadQuarters => HeadQuarters;

        public IDeck Deck { get; }
        public IHand Hand { get; }

        public IResult<IPlayer, int> Draw(int strength)
        {
            var actualStrength = Math.Clamp(strength, 0, Deck.NumberOfCards);
            
            for (var i=0; i < actualStrength; i++)
            {
                var card = Deck.PopCard();
                card.MoveTo(Hand);
            }

            return ResultFactory.Create(this, actualStrength);
        }

        public IResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure)
        {
            HeadQuarters = structure;
            
            return ResultFactory.Create(this, structure);
        }

        
    }
}
