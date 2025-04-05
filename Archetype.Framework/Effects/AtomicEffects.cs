using Archetype.Framework.State;

namespace Archetype.Framework.Effects.Atomic;

[EffectCollection]
public static class AtomicEffect
{
    public record StatChangeResult(string Stat, int Change);
    [Effect("ChangeStat")]
    public static IEffectResult ChangeStat(Atom target, string statKey, int change)
    {
        var stats = target.BaseState.Stats;
        
        if (change == 0)
        {
            return ResultFactory.NoOp();
        }
        
        var currentValue = stats.GetValueOrDefault(statKey);
        
        stats[statKey] = currentValue + change;
        
        return ResultFactory.Atomic(new StatChangeResult(statKey, change));
    }
    
    [Effect("SetStat")]
    public static IEffectResult SetStat(Atom target, string statKey, int value)
    {
        var stats = target.BaseState.Stats;
        
        var currentValue = stats.GetValueOrDefault(statKey);
        
        stats[statKey] = value;
        
        if (currentValue == value)
        {
            return ResultFactory.NoOp();
        }
        
        var change = value - currentValue;
        
        return ResultFactory.Atomic(new StatChangeResult(statKey, change));
    }
    
    public record AddTagResult(string Tag);
    [Effect("AddTag")]
    public static IEffectResult AddTag(Atom target, string tag)
    {
        var tags = target.BaseState.Tags;
        
        if (tag is not { Length: > 0 } || tags.Contains(tag))
        {
            return ResultFactory.Failure();
        }
        
        tags.Add(tag);
        
        return ResultFactory.Atomic(new AddTagResult(tag));
    }
    
    public record RemoveTagResult(string Tag);
    [Effect("RemoveTag")]
    public static IEffectResult RemoveTag(Atom target, string tag)
    {
        var tags = target.BaseState.Tags;
        
        if (tag is not { Length: > 0 } || !tags.Contains(tag))
        {
            return ResultFactory.Failure();
        }
        
        tags.Remove(tag);
        
        return ResultFactory.Atomic(new RemoveTagResult(tag));
    }
    
    public record SetFacetResult(string Key, string[] AddedValues, string[] RemovedValues);
    [Effect("SetFacet")]
    public static IEffectResult SetFacet(Atom target, string key, string[] values)
    {
        var facets = target.BaseState.Facets;
        
        if (key is not { Length: > 0 } || values is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        values = values.Distinct().ToArray();
        
        var currentValues = facets.GetValueOrDefault(key)?.ToArray() ?? Array.Empty<string>();
        
        var addedValues = values.Except(currentValues).ToArray();
        var removedValues = currentValues.Except(values).ToArray();

        if (addedValues.Length == 0 && removedValues.Length == 0)
        {
            return ResultFactory.NoOp();
        }
        
        facets[key] = values;
        
        return ResultFactory.Atomic(new SetFacetResult(key, addedValues, removedValues));
    }
    
    public record RemoveFacetsResult(string Key, string[] Value);
    [Effect("RemoveFacets")]
    public static IEffectResult RemoveFacets(Atom target, string key, string[] valuesToRemove)
    {
        var facets = target.BaseState.Facets;
        
        if (key is not { Length: > 0 } || valuesToRemove is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = facets.GetValueOrDefault(key)?.ToArray();
        
        if (currentValues is null)
        {
            return ResultFactory.Failure();
        }
        
        var newValue = currentValues.Except(valuesToRemove).ToArray();
        var actuallyRemovedValues = currentValues.Except(newValue).ToArray();
        
        if (actuallyRemovedValues.Length == 0)
        {
            return ResultFactory.Failure();
        }
        
        if (newValue.Length == 0)
        {
            facets.Remove(key);
        }
        else
        {
            facets[key] = newValue;
        }
        
        
        return ResultFactory.Atomic(new RemoveFacetsResult(key, actuallyRemovedValues));
    }
    
    [Effect("ClearFacet")]
    public static IEffectResult ClearFacet(Atom target, string key)
    {
        var facets = target.BaseState.Facets;
        
        if (key is not { Length: > 0 } || !facets.TryGetValue(key, out var value))
        {
            return ResultFactory.Failure();
        }
        
        var currentValues = value.ToArray();
        
        facets.Remove(key);
        
        if (currentValues.Length == 0)
        {
            return ResultFactory.Failure();
        }
        
        return ResultFactory.Atomic(new RemoveFacetsResult(key, currentValues));
    }
    
    public record AddFacetsResult(string Key, string[] Value);
    [Effect("AddFacets")]
    public static IEffectResult AddFacets(Atom target, string key, string[] valuesToAdd)
    {
        var facets = target.BaseState.Facets;
        
        if (key is not { Length: > 0 } || valuesToAdd is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = facets.GetValueOrDefault(key)?.ToArray();
        
        if (currentValues is null)
        {
            return ResultFactory.NoOp();
        }
        
        var actuallyAddedValues = valuesToAdd.Except(currentValues).Distinct().ToArray();
        
        var updatedValues = currentValues.Concat(actuallyAddedValues).Distinct().ToArray();
        
        if (actuallyAddedValues.Length == 0)
        {
            return ResultFactory.NoOp();
        }
        
        facets[key] = updatedValues;
        
        return ResultFactory.Atomic(new AddFacetsResult(key, actuallyAddedValues));
    }
}