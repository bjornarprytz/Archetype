using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.GameGraph;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data;

internal static class Extensions
{
    private static readonly Random Random = new (); // TODO: Consider using a seed here
    
    internal static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        var newOrder = collection.ToArray();
        
        var n = newOrder.Length;
        for (var i = 0; i < (n - 1); i++)
        {
            var r = i + Random.Next(n - i);
            
            (newOrder[r], newOrder[i]) = (newOrder[i], newOrder[r]);
        }

        return newOrder;
    }

    internal static IEnumerable<T> PickNUnique<T>(this IEnumerable<T> collection, int n)
    {
        if (n < 1)
            throw new ArgumentException("n must be a non-zero positive int", nameof(n));

        var pool = collection.ToList();

        if (n > pool.Count)
            throw new ArgumentException("n cannot be bigger than the size of the collection", nameof(n));

        if (n == pool.Count)
            return pool;

        var newOrder = pool.Shuffle();

        return newOrder.Take(n);
    }
    
    internal static bool IsEmpty<T>(this IEnumerable<T> items)
    {
        return !items.Any();
    }

    internal static IEnumerable<TResult> NonNull<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, TResult> selector)
    {
        return items.Select(selector).Where(r => r is not null);
    }

    internal static IEnumerable<IBuilding> BuildingsWithKeyword(this IGameState gameState, BuildingKeyword buildingKeyword)
    {
        return gameState.EachBuilding().Where(b => b.HasKeyword(buildingKeyword));
    }

    internal static bool HasKeyword(this IBuildingView building, BuildingKeyword keyword)
    {
        return building.State.Keywords.Contains(keyword);
    }
    
    internal static IEnumerable<IBuilding> EachBuilding(this IGameState gameState)
    {
        return gameState.Map.Nodes.SelectMany(n => n.Buildings());
    }

    internal static IEnumerable<IMapNode> EachEncounter(this IGameState gameState)
    {
        return gameState.Map.Nodes.Where(n => n.Contents.OfType<IEnemy>().Any()).Cast<IMapNode>();
    }

    internal static bool ContainsStation(this IMapNode node)
    {
        return node.Contents.OfType<IBuilding>().Any(b => b.IsStation());
    }
    
    internal static IEnumerable<IEnemy> Enemies(this IMapNode mapNode)
    {
        return mapNode.Contents.OfType<IEnemy>();
    }

    internal static IEnumerable<IBuilding> Buildings(this IMapNode mapNode)
    {
        return mapNode.Contents.OfType<IBuilding>();
    }
    
    internal static IEnumerable<ICrewView> Crew(this IMapNode mapNode)
    {
        return mapNode.Contents.OfType<ICrewView>();
    }
    
    internal static bool IsStation(this IBuilding building)
    {
        return building.State.Types.Any(t => t == BuildingType.Station);
    }

    internal static int TotalMorale(this IMapNode mapNode)
    {
        return mapNode.Buildings().Sum(b => b.State.Morale) + mapNode.Crew().Sum(crew => crew.State.Morale);
    }

    internal static int TotalFear(this IMapNode mapNode)
    {
        return mapNode.Enemies().Sum(e => e.State.Fear);
    }

    internal static void FightBack(this IMapNode mapNode)
    {
        Console.Write("Fighting back! But nothing will happen because it's not implemented yet! TODO: Implement this :)");
    }

    internal static void Connect(this MapNode node, MapNode neighbour)
    {
        node.Connect(neighbour);
        neighbour.Connect(node);
    }
    
    internal static void Disconnect(this MapNode node, MapNode neighbour)
    {
        node.Disconnect(neighbour);
        neighbour.Disconnect(node);
    }

    internal static int Clamp(this int n, int min, int max)
    {
        return Math.Max(min, Math.Min(max, n));
    }
}
