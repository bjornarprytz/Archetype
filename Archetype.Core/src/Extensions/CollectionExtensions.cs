namespace Archetype.Core.Extensions;

public static class CollectionExtensions
{
    private static readonly Random Random = new (); // TODO: Consider using a seed here
        
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
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

    public static IEnumerable<T> PickNUnique<T>(this IEnumerable<T> collection, int n)
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

    public static bool IsEmpty<T>(this IEnumerable<T> items)
    {
        return !items.Any();
    }

    public static T? SecondOrDefault<T>(this IEnumerable<T> source)
    {
        return source.Skip(1).FirstOrDefault();
    }

    public static V GetOrSet<K, V>(this Dictionary<K, V> dictionary, K key)
        where K : notnull
        where V : new()
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
            
        value = new V();
        dictionary[key] = value;

        return value;
    }
}