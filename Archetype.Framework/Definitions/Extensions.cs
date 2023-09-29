namespace Archetype.Framework.Definitions;

public static class Extensions
{
    public static T0 Deconstruct<T0>(this IReadOnlyList<object> collection)
    {
        if (collection.Count != 1)
        {
            throw new InvalidOperationException();
        }
        
        return (collection[0] is T0 t0) ? t0 : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1) Deconstruct<T0, T1>(this IReadOnlyList<object> collection)
    {
        if (collection.Count != 2)
        {
            throw new InvalidOperationException();
        }
        
        return (collection[0] is T0 t0 && collection[1] is T1 t1) ? (t0, t1) : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1, T2 t2) Deconstruct<T0, T1, T2>(this IReadOnlyList<object> collection)
    {
        if (collection.Count != 3)
        {
            throw new InvalidOperationException();
        }
        
        return (collection[0] is T0 t0 && collection[1] is T1 t1 && collection[2] is T2 t2) ? (t0, t1, t2) : throw new InvalidOperationException();
    }
    
    public static (T0 t0, T1 t1, T2 t2, T3 t3) Deconstruct<T0, T1, T2, T3>(this IReadOnlyList<object> collection)
    {
        if (collection.Count != 4)
        {
            throw new InvalidOperationException();
        }
        
        return (collection[0] is T0 t0 && collection[1] is T1 t1 && collection[2] is T2 t2 && collection[3] is T3 t3) ? (t0, t1, t2, t3) : throw new InvalidOperationException();
    }
}