using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms.Infrastructure;

public interface IWorld : IZone<ICard>
{
    public IDrawPile WorldDeck { get; }
    public IEnumerable<ICard> OpenLocations { get; }
}