using Archetype.Core.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;
using Archetype.View.Infrastructure.State;

namespace Archetype.Core.Infrastructure;

public interface IMap : IMapFront
{
    new IEnumerable<IMapNode> Nodes { get; } 

    void AddNodes(IEnumerable<IMapNode> nodes);
}

internal class Map : IMap
{
    private readonly List<IMapNode> _nodes = new();

    public IEnumerable<IMapNode> Nodes => _nodes;
    public void AddNodes(IEnumerable<IMapNode> nodes)
    {
        _nodes.AddRange(nodes);
    }

    IEnumerable<IMapNodeFront> IMapFront.Nodes => Nodes;
}