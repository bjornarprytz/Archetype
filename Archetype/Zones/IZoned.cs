

namespace Archetype
{
    public interface IZoned<T> where T : GamePiece
    {
        Zone<T> CurrentZone { get; }

        void MoveTo(Zone<T> newZone);
    }
}
