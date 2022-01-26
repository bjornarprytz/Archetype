using Archetype.View.Atoms.Zones;

namespace Archetype.View.Atoms;

public interface IPieceFront : IGameAtomFront
{
    string Name { get; }
    IZoneFront CurrentZone { get; }
}