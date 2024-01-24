namespace Archetype.Framework.Extensions;

public static class CollectionExtensions
{
    public static void MergeInPlace<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second) 
        where TKey : notnull
    {
        foreach (var keyVal in second)
        {
            first[keyVal.Key] = keyVal.Value;
        }
    }
}