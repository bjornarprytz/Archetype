using Archetype.View.Atoms.Zones;

namespace Archetype.View.Atoms;

public interface IZonedFront
{
    IZoneFront CurrentZone { get; }
}