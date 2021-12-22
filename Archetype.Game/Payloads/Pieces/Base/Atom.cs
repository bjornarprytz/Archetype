using System;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IGameAtomFront
    {
        Guid Guid { get; }
    }
    
    internal interface IGameAtom : IGameAtomFront
    {
        string Name { get; }
        IGameAtom Owner { get; }
    }

    internal abstract class Atom : IGameAtom
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