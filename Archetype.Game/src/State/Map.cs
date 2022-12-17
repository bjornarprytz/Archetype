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

internal class Node : Atom, INode
{
    private readonly Dictionary<Guid, Node> _neighbors = new();
    private readonly Dictionary<Guid, IUnit> _units = new();
    public IEnumerable<IUnit> Contents => _units.Values;
    public IEnumerable<INode> Neighbors => _neighbors.Values;
    
    public void AddUnit(IUnit unit)
    {
        // TODO: This is probably better in an abstract class
        _units.Add(unit.Id, unit);
    }
    
    public void RemoveUnit(IUnit unit)
    {
        _units.Remove(unit.Id);
    }
    
    public void AddNeighbor(Node node)
    {
        if (node == this || _neighbors.ContainsKey(node.Id))
            return;
        
        _neighbors.Add(node.Id, node);
    }
}