﻿namespace Archetype.Framework.Extensions;

public static class CollectionExtensions
{
    public static T0 Deconstruct<T0>(this IEnumerable<object> collection)
    {
        var collectionList = collection.ToList();
        
        if (collectionList.Count != 1)
        {
            throw new InvalidOperationException();
        }
        
        return (collectionList[0] is T0 t0) 
            ? t0 : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1) Deconstruct<T0, T1>(this IEnumerable<object> collection)
    {
        var collectionList = collection.ToList();
        
        if (collectionList.Count != 2)
        {
            throw new InvalidOperationException();
        }
        
        return (collectionList[0] is T0 t0 
                && collectionList[1] is T1 t1) 
            ? (t0, t1) : throw new InvalidOperationException();
    }

    public static (T0 t0, T1 t1, T2 t2) Deconstruct<T0, T1, T2>(this IEnumerable<object> collection)
    {
        var collectionList = collection.ToList();
        
        if (collectionList.Count != 3)
        {
            throw new InvalidOperationException();
        }
        
        return (collectionList[0] is T0 t0 
                && collectionList[1] is T1 t1 
                && collectionList[2] is T2 t2) 
            ? (t0, t1, t2) : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1, T2 t2, T3 t3) Deconstruct<T0, T1, T2, T3>(this IEnumerable<object> collection)
    {
        var collectionList = collection.ToList();
        
        if (collectionList.Count != 4)
        {
            throw new InvalidOperationException();
        }
        
        return (collectionList[0] is T0 t0 
                && collectionList[1] is T1 t1 
                && collectionList[2] is T2 t2 
                && collectionList[3] is T3 t3) 
            ? (t0, t1, t2, t3) : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1, T2 t2, T3 t3, T4 t4) Deconstruct<T0, T1, T2, T3, T4>(this IEnumerable<object> collection)
    {
        var collectionList = collection.ToList();
        
        if (collectionList.Count != 5)
        {
            throw new InvalidOperationException();
        }
        
        return (collectionList[0] is T0 t0 
                && collectionList[1] is T1 t1 
                && collectionList[2] is T2 t2 
                && collectionList[3] is T3 t3 
                && collectionList[4] is T4 t4) 
            ? (t0, t1, t2, t3, t4) : throw new InvalidOperationException();
    }
}