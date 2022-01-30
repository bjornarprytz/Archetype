using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms.Base
{
    internal abstract class Piece<T> : Atom, IPiece<T>
        where T : IPiece
    {
        protected Piece(string name, IGameAtom owner) : base(owner)
        {
            Name = name;
        }

        public string Name { get; }

        public IZone<T> CurrentZone { get; private set; }

        public IEffectResult<IPiece<T>, IZone<T>> MoveTo(IZone<T> zone)
        {
            if (zone == CurrentZone)
                return ResultFactory.Null<IPiece<T>, IZone<T>>(this);

            CurrentZone?.Remove(Self);
            CurrentZone = zone;
            CurrentZone?.Add(Self);

            return ResultFactory.Create(this, CurrentZone);
        }

        protected abstract T Self { get; }
        IZoneFront IPieceFront.CurrentZone => CurrentZone;
    }
}