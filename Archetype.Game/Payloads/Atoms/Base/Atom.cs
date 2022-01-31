using System;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IGameAtom : IGameAtomFront
    {
        new IGameAtom Owner { get; }

        IEffectResult<IGameAtom, IGameAtom> SetOwner(IGameAtom newOwner);
    }

    public abstract class Atom : IGameAtom
    {
        protected Atom(IGameAtom owner=default)
        {
            Guid = Guid.NewGuid();
            Owner = owner ??= this;
        }

        public Guid Guid { get; }
        public IGameAtom Owner { get; private set; }
        public IEffectResult<IGameAtom, IGameAtom> SetOwner(IGameAtom newOwner)
        {
            if (Owner == newOwner)
                return ResultFactory.Null<IGameAtom, IGameAtom>();
            
            Owner = newOwner;

            return ResultFactory.Create(this, newOwner);
        }

        IGameAtomFront IGameAtomFront.Owner => Owner;
    }
}