using Archetype.View.Atoms.Zones;

namespace Archetype.View.Infrastructure.State;

public interface IMapFront
{
    IEnumerable<IMapNodeFront> Nodes { get; }
}