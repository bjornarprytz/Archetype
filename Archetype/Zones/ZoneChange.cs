
namespace Archetype
{
    public delegate void ZoneChange<T>(Zone<T> from, Zone<T> to) where T : GamePiece;
}
