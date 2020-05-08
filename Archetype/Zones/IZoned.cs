

namespace Archetype
{
    public interface IZoned<T> where T : GamePiece
    {
        event ZoneChange<T> OnZoneChanged;
        Zone<T> CurrentZone { get; }

        void MoveTo(Zone<T> newZone);
    }
}
