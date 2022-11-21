using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms.Infrastructure;

public interface ILocation
{
    public IEnumerable<ILocationNode> Nodes { get; }
}