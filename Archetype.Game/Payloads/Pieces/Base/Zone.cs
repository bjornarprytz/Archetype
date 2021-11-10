using System;
using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZone : IGameAtom
    {
        IGameAtom GetGamePiece(Guid guid);
    }

    public interface IZone<out T> : IZone
        where T : IGameAtom, IZoned<T>
    {
        IEnumerable<T> Contents { get; }

        T GetTypedPiece(Guid guid);
    }

    public abstract class Zone<TContents> : Atom, IZone<TContents>
        where TContents : IGameAtom, IZoned<TContents>
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
        
        protected void AddPiece(TContents gamePiece)
        {
            _contents.Add(gamePiece.Guid, gamePiece);
            gamePiece.CurrentZone = this;

        }

        protected void RemovePiece(TContents gamePiece)
        {
            gamePiece.CurrentZone = default;
            _contents.Remove(gamePiece.Guid);
        }
    }
}