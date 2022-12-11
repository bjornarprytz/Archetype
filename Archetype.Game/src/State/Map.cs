using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Game.State;

internal class Map : IMap
{
    private readonly Dictionary<Guid, Node> _nodes = new();
    public IEnumerable<INode> Nodes => _nodes.Values;


    public void AddNode(Node node)
    {
        _nodes.Add(node.Id, node);
    }
    
    public void ConnectNodes(Guid nodeId1, Guid nodeId2)
    {
        if (
            !_nodes.TryGetValue(nodeId1, out var node1)
        || 
            !_nodes.TryGetValue(nodeId2, out var node2))
        {
            throw new ArgumentException($"Node with id {nodeId1} does not exist");
        }
        
        node1.AddNeighbor(node2);
        node2.AddNeighbor(node1);
    }
    
    public bool IsFullyConnected()
    {
        if (_nodes.Count == 0)
        {
            return true;
        }
        
        var visited = new HashSet<Guid>();
        var stack = new Stack<INode>();
        stack.Push(_nodes.Values.First());
        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            if (visited.Contains(currentNode.Id))
            {
                continue;
            }
            
            visited.Add(currentNode.Id);
            foreach (var neighbor in currentNode.Neighbors)
            {
                stack.Push(neighbor);
            }
        }

        return visited.Count == _nodes.Count;
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