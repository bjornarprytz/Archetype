using System;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IGameAtom
    {
        string Name { get; }
        Guid Guid { get; }
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