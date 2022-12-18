using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Game.State;

internal class Map : IMap
{
    private readonly List<Node> _nodes = new();
    public IEnumerable<INode> Nodes => _nodes;


    public void AddNode(Node node)
    {
        _nodes.Add(node);
    }
    
    public void ConnectNodes(int n1, int n2)
    {
        if (n1 >= _nodes.Count || n2 >= _nodes.Count || n1 < 0 || n2 < 0)
            throw new ArgumentOutOfRangeException();
        
        if (n1 == n2)
            throw new ArgumentException("Cannot connect a node to itself");

        var node1 = _nodes[n1];
        var node2 = _nodes[n2];
        
        node1.AddNeighbor(node2);
        node2.AddNeighbor(node1);
    }
}

internal class Node : Zone<IUnit>, INode
{
    private readonly Dictionary<Guid, Node> _neighbors = new();
    public IEnumerable<INode> Neighbors => _neighbors.Values;

    public void AddNeighbor(Node node)
    {
        if (node == this || _neighbors.ContainsKey(node.Id))
            return;
        
        _neighbors.Add(node.Id, node);
    }
}