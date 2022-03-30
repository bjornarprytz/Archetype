namespace Archetype.Prototype1Data
{
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

        internal static bool ContainsBase(this IMapNode node)
        {
            return node.Building?.IsBase ?? false;
        }

        internal static IEnumerable<Enemy> EachEnemy(this IGameState gameState)
        {
            return gameState.Map.Nodes.SelectMany(n => n.Enemies).OfType<Enemy>();
        }
        
        internal static IEnumerable<Enemy> EachRoamingEnemy(this IGameState gameState)
        {
            return gameState.Map.Nodes
                .SelectMany(n => n.Enemies)
                .OfType<Enemy>()
                .Where(IsRoaming);
        }

        internal static bool IsRoaming(this IEnemy enemy)
        {
            return enemy.IsOnTheMap() && !enemy.IsEngaged() && !enemy.Node.ContainsBase();
        }
        
        internal static bool IsEngaged(this IEnemy enemy)
        {
            return enemy.Building is not null;
        }

        internal static bool IsOnTheMap(this IEnemy enemy)
        {
            return enemy.Node is not null;
        }

        internal static bool IsDead(this IEnemy enemy)
        {
            return !enemy.IsAlive();
        }
        
        internal static bool IsAlive(this IEnemy enemy)
        {
            return enemy.Health > 0;
        }

        internal static IDictionary<MapNode, MapNode> PathToBase(this IMap map)
        {
            if (map.Nodes.FirstOrDefault(n => n.ContainsBase()) is not MapNode targetNode)
            {
                throw new ArgumentException("Cannot generate path for map without a base!");
            }
            
            var path = new Dictionary<MapNode, MapNode> { { targetNode, targetNode } };

            UpdateNeighbours(targetNode);

            return path;

            void UpdateNeighbours(MapNode node)
            {
                var neighboursToUpdate = new List<MapNode>();
                
                foreach (var neighbour in node.Neighbours.OfType<MapNode>())
                {
                    if (path.ContainsKey(neighbour))
                        continue;

                    path.Add(neighbour, node);
                    neighboursToUpdate.Add(neighbour);
                }
                
                foreach (var neighbour in neighboursToUpdate)
                {
                    UpdateNeighbours(neighbour);
                }
            }
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
}