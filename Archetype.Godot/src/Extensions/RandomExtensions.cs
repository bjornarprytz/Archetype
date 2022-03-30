using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Archetype.Godot.Extensions;

public static class RandomExtensions
{
    private static readonly Random Random = new(); // TODO: Consider using a seed here
        
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
    
    internal static T PickOneRandom<T>(this IEnumerable<T> collection)
    {
        return collection.PickNUnique(1).FirstOrDefault();
    }
    
    internal static T PickOneRandom<T>(this T[] array)
    {
        if (array.Length < 1)
            throw new ArgumentException("can't pick from an empty array"); 
        
        return array[Random.Next(array.Length-1)];
    }
    
    internal static Vector3 RandomInCenteredBounds(this Vector3 bounds)
    {
        var x = (float) (Random.NextDouble() - 0.5f ) * bounds.x;
        var y = (float) (Random.NextDouble() - 0.5f ) * bounds.y;
        var z = (float) (Random.NextDouble() - 0.5f ) * bounds.z;

        return new Vector3(x, y, z);
    }
    
    internal static Vector3 RandomInBounds(this Vector3 bounds)
    {
        var x = (float) Random.NextDouble() * bounds.x;
        var y = (float) Random.NextDouble() * bounds.y;
        var z = (float) Random.NextDouble() * bounds.z;

        return new Vector3(x, y, z);
    }
}