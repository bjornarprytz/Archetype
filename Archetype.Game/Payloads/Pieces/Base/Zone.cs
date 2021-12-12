using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Archetype.Game.Exceptions;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZone : IGameAtom
    {
        IGameAtom GetGamePiece(Guid guid);
    }

    public interface IZone<T> : IZone
        where T : IGameAtom, IZoned<T>
    {
        IEnumerable<T> Contents { get; }

        T GetTypedPiece(Guid guid);

        void _Place(T atom); // TODO: Hide this method to avoid misuse (use IZoned.MoveTo instead)
    }

    public abstract class Zone<TContents> : Atom, IZone<TContents>
        where TContents : IZoned<TContents>
    {
        private readonly Dictionary<Guid, TContents> _contents = new();
        
        protected Zone(IGameAtom owner=default) : base(owner) { }
        
        public IEnumerable<TContents> Contents => _contents.Values;
        
        public virtual TContents GetTypedPiece(Guid guid)
        {
            return _contents[guid];
        }

        public virtual IGameAtom GetGamePiece(Guid guid)
        {
            return _contents[guid];
        }
        
        public void _Place(TContents atom)
        {
            if (atom.CurrentZone != this)
                throw new ZonePlacementException(this,
                    $"{nameof(atom.CurrentZone)} should be set before {nameof(_Place)} is called");
            
            _contents.Add(atom.Guid, atom);

            atom.Transition
                .Where(t => t.From == this && t.To == this)
                .Take(1)
                .Subscribe(HandleZoneTransition);
        }

        private void HandleZoneTransition(ZoneTransition<TContents> zoneTransition)
        {
            _contents.Remove(zoneTransition.Who.Guid);
        }
    }
}