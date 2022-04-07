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

    internal static IEnumerable<Building> BuildingsWithKeyword(this IGameState gameState, Keyword keyword)
    {
        return gameState.EachBuilding().Where(b => b.Keywords.Contains(keyword));
    }
    
    internal static IEnumerable<Building> EachBuilding(this IGameState gameState)
    {
        return gameState.Map.Nodes.NonNull(n => n.Building).OfType<Building>();
    }

    internal static IEnumerable<IMapNode> EachEncounter(this IGameState gameState)
    {
        return gameState.Map.Nodes.Where(n => n.Wave is { });
    }

    internal static bool ContainsStation(this IMapNode node)
    {
        return node.Building?.IsStation ?? false;
    }

    internal static int TotalDefense(this IMapNode mapNode)
    {
        return (mapNode.Building?.Defense ?? 0) + mapNode.Defenders.Sum(crew => crew.Defense);
    }

    internal static int TotalAttack(this IMapNode mapNode)
    {
        return mapNode.Wave?.Strength ?? 0;
    }

    internal static void  FightBack(this IMapNode mapNode)
    {
        Console.Write("Fighting back! But nothing will happen because it's not implemented yet! TODO: Implement this :)");
    }

    internal static void Rout(this IMapNode mapNode)
    {
        mapNode.Building?.Destroy();
    }
    
    internal static void Damage(this IMapNode mapNode)
    {
        mapNode.Building?.Damage();
    }

    internal static void Connect(this MapNode node, MapNode neighbour)
    {
        node.AddNeighbour(neighbour);
        neighbour.AddNeighbour(node);
    }
    
    internal static void Disconnect(this MapNode node, MapNode neighbour)
    {
        node.RemoveNeighbour(neighbour);
        neighbour.RemoveNeighbour(node);
    }

    internal static int Clamp(this int n, int min, int max)
    {
        return Math.Max(min, Math.Min(max, n));
    }
}
