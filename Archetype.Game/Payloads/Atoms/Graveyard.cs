using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IGraveyard : IZone<IUnit>, // TODO: This should be of ICreature, but I might have to do something smarter with the interfaces
            IGraveyardFront 
    {
        new IObservable<IAtomMutation<IGraveyard>> OnMutation { get; }
    }

    internal class Graveyard : Zone<IUnit>, IGraveyard
    {
        private readonly Subject<IAtomMutation<IGraveyard>> _mutation = new();
        
        public Graveyard(IGameAtom owner) : base(owner) { }
        public IEnumerable<ICreatureFront> Creatures => Contents.OfType<ICreature>();
        public override IObservable<IAtomMutation> OnMutation => _mutation;
        IObservable<IAtomMutation<IGraveyard>> IGraveyard.OnMutation => _mutation;
    }
}