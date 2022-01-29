using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IZone<T> : IGameAtom, IZoneFront
        where T : IPiece
    {
        new IEnumerable<T> Contents { get; }

        void Add(T piece);
        void Remove(T piece);
    }

    internal abstract class Zone<T> : Atom, IZone<T>
        where T : IPiece
    {
        private readonly Dictionary<Guid, T> _contents = new();

        protected Zone(IGameAtom owner) : base(owner) { }
        
        public IEnumerable<T> Contents => _contents.Values;

        IEnumerable<IGameAtomFront> IZoneFront.Contents => _contents.Values.OfType<IGameAtomFront>();
        
        public void Add(T piece)
        {
            _contents.Add(piece.Guid, piece);
        }

        public void Remove(T piece)
        {
            _contents.Remove(piece.Guid);
        }
    }
}