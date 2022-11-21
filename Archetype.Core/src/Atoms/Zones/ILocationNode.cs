namespace Archetype.Core.Atoms.Zones;

public interface ILocationNode : IZone
{
    public IEnumerable<ILocationNode> Neighbors { get; }
}