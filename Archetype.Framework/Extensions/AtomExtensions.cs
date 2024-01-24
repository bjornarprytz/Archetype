using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class AtomExtensions
{
    public static T? GetState<T>(this IAtom atom, string key)
    {
        if (!atom.State.TryGetValue(key, out var value)) return default;
        
        if (value is T typedValue)
            return typedValue;
            
        throw new InvalidOperationException($"State key {key} is not of type {typeof(T).Name}");

    }
    
    public static bool HasTag(this IAtom atom, string tag)
    {
        return atom.Tags.ContainsKey(tag);
    }
    public static string? GetTag(this IAtom atom, string tag)
    {
        return atom.Tags.TryGetValue(tag, out var value) ? value : null;
    }
    
    public static bool HasStat(this IAtom atom, string stat)
    {
        return atom.Stats.ContainsKey(stat);
    }
    
    public static int? GetStatValue(this IAtom atom, string stat)
    {
        return atom.Stats.TryGetValue(stat, out var value) ? value : null;
    }
    
    public static IAtomProvider ToAtomProvider(this IZone zone)
    {
        return new AtomProvider(_ => zone.Atoms, (atom, _) => atom.CurrentZone == zone);
    }
    
}