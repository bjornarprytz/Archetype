namespace Archetype.Prototype1Data
{
    internal static class Extensions
    {
        private static readonly Random Random = new Random(); // TODO: Consider using a seed here
        
        internal static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var array = collection.ToArray();
            
            var n = array.Length;
            for (var i = 0; i < (n - 1); i++)
            {
                var r = i + Random.Next(n - i);
                
                (array[r], array[i]) = (array[i], array[r]);
            }

            return array;
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

        internal static bool ContainsBase(this IMapNode node)
        {
            return node.Building?.IsBase ?? false;
        }

        internal static IEnumerable<Enemy> EachEnemy(this IGameState gameState)
        {
            return gameState.Map.Nodes.SelectMany(n => n.Enemies).OfType<Enemy>();
        }
        
        internal static IEnumerable<Enemy> EachIncomingEnemy(this IGameState gameState)
        {
            return gameState.Map.Nodes
                .SelectMany(n => n.Enemies)
                .OfType<Enemy>()
                .Where( e => e.Node is not null && e.Node.ContainsBase());
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
    }
}