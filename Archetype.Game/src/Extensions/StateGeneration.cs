using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;
using Archetype.Game.State;

namespace Archetype.Game.Extensions;

internal static class StateGeneration
{
    public static IMap GenerateMap(Random random, int numNodes)
    {
        var map = new Map();

        var nodes = Enumerable.Range(0, numNodes).Select(_ => new Node()).ToList();

        foreach (var node in nodes)
        {
            map.AddNode(node);
        }

        if (numNodes == 0)
        {
            return map;
        }
        
        // TODO: Maybe connect map randomly
        for (var i = 1; i < numNodes; i++)
        {
            map.ConnectNodes(i-1, i % numNodes); 
        }

        return map;
    }
    
    public static IPlayer GeneratePlayer(Random random)
    {
        return new Player();
    }
}