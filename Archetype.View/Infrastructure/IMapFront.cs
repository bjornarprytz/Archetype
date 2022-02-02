using Archetype.View.Atoms.Zones;

namespace Archetype.View.Infrastructure;

public interface IMapFront
{
    IEnumerable<IMapNodeFront> Nodes { get; }
}