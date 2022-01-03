using System;
using Archetype.View.Atoms;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IGameAtom : IGameAtomFront
    {
        string Name { get; }
        IGameAtom Owner { get; }
    }

    public abstract class Atom : IGameAtom
    {
        protected Atom(IGameAtom owner=default)
        {
            Guid = Guid.NewGuid();
            Owner = owner ??= this;
        }

        public string Name { get; set; }
        public Guid Guid { get; }
        public IGameAtom Owner { get; }
    }
}