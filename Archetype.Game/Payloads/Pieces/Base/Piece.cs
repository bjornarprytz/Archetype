using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public abstract class Piece<T> : Atom, IZoned<T> where T : IGameAtom, IZoned<T>
    {
        protected Piece(IGameAtom owner) : base(owner)
        {
            
        }

        public IZone<T> CurrentZone { get; set; }
    }
}