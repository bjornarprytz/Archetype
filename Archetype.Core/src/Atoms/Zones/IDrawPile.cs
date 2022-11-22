using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface IDrawPile : IZone<ICard>
{
    public int Count { get; }
}