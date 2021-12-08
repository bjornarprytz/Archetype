using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public abstract class Piece<T> : Atom, IZoned<T> 
        where T : class, IGameAtom, IZoned<T>
    {
        protected Piece(IGameAtom owner) : base(owner) { }

        public IZone<T> CurrentZone { get; set; }
        public IEffectResult<IZoned<T>, IZone<T>> MoveTo(IZone<T> zone)
        {
            if (zone == CurrentZone)
                return ResultFactory.Null<IZoned<T>, IZone<T>>(this);
            
            CurrentZone = zone;

            return ResultFactory.Create(this, zone);
        }
    }
}