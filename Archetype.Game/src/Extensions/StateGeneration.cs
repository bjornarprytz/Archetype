using Archetype.Core.Atoms.Zones;
using Archetype.Game.State;

namespace Archetype.Game.Extensions;

internal static class StateGeneration
{
    public static IMap GenerateMap(Random random, int numNodes)
    {
        // TODO: Generate a map with the given number of nodes.
        var map = new Map();

        var nodes = Enumerable.Range(0, numNodes).Select(_ => new Node()).ToList();

        foreach (var node in nodes)
        {
            map.AddNode(node);
        }
        
        // TODO: Add some connections between nodes. 
        
        if (!map.IsFullyConnected())
        {
            throw new NotImplementedException(); // TODO: Decide what to do if the map isn't fully connected. Just retry?
        }

        return map;
    }
    
    
}