using Archetype.Framework.State;

namespace Archetype.Framework.Effects.Atomic;

[EffectCollection]
public static class AtomicEffect
{
    [Effect("ChangeStat")]
    public static EffectResult ChangeStat(Atom target, string statKey, int change)
    {
        if (change == 0)
        {
            return ResultFactory.NoOp([target, statKey, change]);
        }
        
        var stats = target.State.Stats;
        
        var currentValue = stats.GetValueOrDefault(statKey);
        
        stats[statKey] = currentValue + change;
        
        return ResultFactory.Atomic([target, statKey, change]);
    }
    
    [Effect("SetStat")]
    public static EffectResult SetStat(Atom target, string statKey, int value)
    {
        var stats = target.State.Stats;
        
        if (stats.TryGetValue(statKey, out var currentValue) && currentValue == value)
        {
            return ResultFactory.NoOp([target, statKey, value]);
        }
        
        stats[statKey] = value;
        
        return ResultFactory.Atomic([target, statKey, value]);
    }
    
    [Effect("AddTag")]
    public static EffectResult AddTag(Atom target, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !target.State.Tags.Add(tag))
        {
            return ResultFactory.NoOp([target, tag]);
        }

        return ResultFactory.Atomic([target, tag]);
    }
    
    [Effect("RemoveTag")]
    public static EffectResult RemoveTag(Atom target, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !target.State.Tags.Remove(tag))
        {
            return ResultFactory.NoOp([target, tag]);
        }
        
        return ResultFactory.Atomic([target, tag]);
    }
    
    [Effect("SetFacet")]
    public static EffectResult SetFacet(Atom target, string key, string[] values)
    {
        var facets = target.State.Facets;
        
        var distinctValues = values.Distinct().ToArray();

        if (facets.TryGetValue(key, out var currentValues) && currentValues.SequenceEqual(distinctValues))
        {
            return ResultFactory.NoOp([target, key, distinctValues]);
        }

        if (distinctValues.Length == 0)
        {
            if (facets.Remove(key))
            {
                return ResultFactory.Atomic([target, key, distinctValues]);
            }
            else
            {
                return ResultFactory.NoOp([target, key, distinctValues]);
            }
        }
        
        target.State.Facets[key] = distinctValues;
        
        return ResultFactory.Atomic([target, key, distinctValues]);
    }
    
    [Effect("RemoveFacets")]
    public static EffectResult RemoveFacets(Atom target, string key, string[] valuesToRemove)
    {
        var facets = target.State.Facets;
        
        var currentValues = facets.GetValueOrDefault(key)?.ToArray();
        
        if (currentValues is not { Length: > 0})
        {
            return ResultFactory.NoOp([target, key, valuesToRemove]);
        }
        
        var actuallyRemovedValues = currentValues.Intersect(valuesToRemove).Distinct().ToArray();
        
        if (actuallyRemovedValues.Length == 0)
        {
            return ResultFactory.NoOp([target, key, valuesToRemove]);
        }
        
        var newValue = currentValues.Except(valuesToRemove).ToArray();
        
        if (newValue.Length == 0)
        {
            facets.Remove(key);
        }
        else
        {
            facets[key] = newValue;
        }
        
        return ResultFactory.Atomic([target, key, actuallyRemovedValues]);
    }
    
    [Effect("ClearFacet")]
    public static EffectResult ClearFacet(Atom target, string key)
    {
        var facets = target.State.Facets;

        if (!target.State.Facets.Remove(key))
        {
            return ResultFactory.NoOp([target, key]);
        }
        
        return ResultFactory.Atomic([target, key]);
    }
    
    [Effect("AddFacets")]
    public static EffectResult AddFacets(Atom target, string key, string[] valuesToAdd)
    {
        var facets = target.State.Facets;
        
        var currentValues = facets.GetValueOrDefault(key)?.ToArray();
        
        if (currentValues is null)
        {
            return ResultFactory.NoOp([target, key, valuesToAdd]);
        }
        
        var actuallyAddedValues = valuesToAdd.Except(currentValues).Distinct().ToArray();
        
        var updatedValues = currentValues.Concat(actuallyAddedValues).Distinct().ToArray();
        
        if (actuallyAddedValues.Length == 0)
        {
            return ResultFactory.NoOp([target, key, valuesToAdd]);
        }
        
        facets[key] = updatedValues;
        
        return ResultFactory.Atomic([target, key, actuallyAddedValues]);
    }
}