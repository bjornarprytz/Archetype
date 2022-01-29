using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IDiscardPile : IZone<ICard>, IDiscardPileFront
    {
        new IObservable<IAtomMutation<IDiscardPile>> OnMutation { get; }
    }
    
    internal class DiscardPile : Zone<ICard>, IDiscardPile
    {
        private readonly Subject<IAtomMutation<IDiscardPile>> _mutation = new ();
        public DiscardPile(IGameAtom owner) : base(owner) { }

        public IEnumerable<ICardFront> Cards => Contents;
        public override IObservable<IAtomMutation> OnMutation => _mutation;
        IObservable<IAtomMutation<IDiscardPile>> IDiscardPile.OnMutation => _mutation;
    }
}
