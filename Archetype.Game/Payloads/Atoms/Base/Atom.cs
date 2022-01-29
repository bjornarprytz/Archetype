using System;
using Archetype.View.Atoms;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IGameAtom : IGameAtomFront
    {
        new IGameAtom Owner { get; }
        IObservable<IAtomMutation> OnMutation { get; } // TODO: Call this everywhere
    }

    public abstract class Atom : IGameAtom
    {
        protected Atom(IGameAtom owner=default)
        {
            Guid = Guid.NewGuid();
            Owner = owner ??= this;
        }
        
        public Guid Guid { get; }
        public IGameAtom Owner { get; }
        
        IGameAtomFront IGameAtomFront.Owner => Owner;

        public abstract IObservable<IAtomMutation> OnMutation { get; }
    }
}