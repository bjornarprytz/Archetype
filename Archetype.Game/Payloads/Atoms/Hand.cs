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
    public interface IHand : IZone<ICard>, IHandFront
    {
        new IObservable<IAtomMutation<IHand>> OnMutation { get; }
    }

    internal class Hand : Zone<ICard>, IHand
    {
        private readonly Subject<IAtomMutation<IHand>> _mutation = new();
        public Hand(IGameAtom owner) : base(owner) { }
        public IEnumerable<ICardFront> Cards => Contents;
        public override IObservable<IAtomMutation> OnMutation => _mutation;
        IObservable<IAtomMutation<IHand>> IHand.OnMutation => _mutation;
    }
}
