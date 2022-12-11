using Archetype.Core.Atoms.Cards;

namespace Archetype.Core.Atoms.Zones;

public interface IMap
{
    public IEnumerable<INode> Nodes { get; }
}

public interface INode : IZone<IUnit>
{
    public IEnumerable<INode> Neighbors { get; }
}