using System.Security.Cryptography;
using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Effects.Atomic;

[EffectCollection]
public static class AtomicEffect
{
    public record StatChangeResult(string Stat, int Change);
    [Effect("ChangeStat")]
    public static IEffectResult ChangeStat(IHasStats target, string stat, int change)
    {
        if (change == 0)
        {
            return ResultFactory.NoOp();
        }
        
        var currentValue = target.GetStat(stat) ?? 0;
        
        target.SetStat(stat, currentValue + change);
        
        return ResultFactory.Atomic(new StatChangeResult(stat, change));
    }
    
    [Effect("SetStat")]
    public static IEffectResult SetStat(IHasStats target, string stat, int value)
    {
        var currentValue = target.GetStat(stat) ?? 0;
        
        if (currentValue == value)
        {
            return ResultFactory.NoOp();
        }
        
        target.SetStat(stat, value);
        
        var change = value - currentValue;
        
        return ResultFactory.Atomic(new StatChangeResult(stat, change));
    }
    
    public record AddTagResult(string Tag);
    [Effect("AddTag")]
    public static IEffectResult AddTag(IHasTags target, string tag)
    {
        if (tag is not { Length: > 0 } || target.HasTag(tag))
        {
            return ResultFactory.NoOp();
        }
        
        target.AddTag(tag);
        
        return ResultFactory.Atomic(new AddTagResult(tag));
    }
    
    public record RemoveTagResult(string Tag);
    [Effect("RemoveTag")]
    public static IEffectResult RemoveTag(IHasTags target, string tag)
    {
        if (tag is not { Length: > 0 } || !target.HasTag(tag))
        {
            return ResultFactory.NoOp();
        }
        
        target.RemoveTag(tag);
        
        return ResultFactory.Atomic(new RemoveTagResult(tag));
    }
    
    public record SetFacetResult(string Key, string[] AddedValues, string[] RemovedValues);
    [Effect("SetFacet")]
    public static IEffectResult SetFacet(IHasFacets target, string key, string[] values)
    {
        if (key is not { Length: > 0 } || values is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        values = values.Distinct().ToArray();
        
        var currentValues = target.GetFacet(key)?.ToArray() ?? Array.Empty<string>();
        
        var addedValues = values.Except(currentValues).ToArray();
        var removedValues = currentValues.Except(values).ToArray();

        if (addedValues.Length == 0 && removedValues.Length == 0)
        {
            return ResultFactory.NoOp();
        }
        
        target.SetFacet(key, values);
        
        return ResultFactory.Atomic(new SetFacetResult(key, addedValues, removedValues));
    }
    
    public record RemoveFacetsResult(string Key, string[] Value);
    [Effect("RemoveFacets")]
    public static IEffectResult RemoveFacets(IHasFacets target, string key, string[] valuesToRemove)
    {
        if (key is not { Length: > 0 } || valuesToRemove is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = target.GetFacet(key)?.ToArray();
        
        if (currentValues is null)
        {
            return ResultFactory.NoOp();
        }
        
        var newValue = currentValues.Except(valuesToRemove).ToArray();
        var actuallyRemovedValues = currentValues.Except(newValue).ToArray();
        
        if (actuallyRemovedValues.Length == 0)
        {
            return ResultFactory.NoOp();
        }
        
        target.SetFacet(key, newValue);
        
        return ResultFactory.Atomic(new RemoveFacetsResult(key, actuallyRemovedValues));
    }
    
    [Effect("ClearFacet")]
    public static IEffectResult ClearFacet(IHasFacets target, string key)
    {
        if (key is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = target.GetFacet(key)?.ToArray();
        
        if (currentValues is null || currentValues.Length == 0)
        {
            return ResultFactory.NoOp();
        }
        
        target.RemoveFacet(key);
        
        return ResultFactory.Atomic(new RemoveFacetsResult(key, currentValues));
    }
    
    public record AddFacetsResult(string Key, string[] Value);
    [Effect("AddFacets")]
    public static IEffectResult AddFacets(IHasFacets target, string key, string[] valuesToAdd)
    {
        if (key is not { Length: > 0 } || valuesToAdd is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = target.GetFacet(key)?.ToArray();
        
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
        
        target.SetFacet(key, updatedValues);
        
        return ResultFactory.Atomic(new AddFacetsResult(key, actuallyAddedValues));
    }
    
    public record struct ChangeZoneResult(Guid Target, Guid? From, Guid To);
    [Effect("ChangeZone")]
    public static IEffectResult ChangeZone(IAtom target, IZone zone)
    {
        if (target.Zone == zone)
        {
            return ResultFactory.NoOp();
        }
        
        var currentZone = target.Zone;

        currentZone?.RemoveAtom(target);
        
        target.Zone = zone;
        
        zone.AddAtom(target);
        
        return ResultFactory.Atomic(new ChangeZoneResult(target.Id, currentZone?.Id, zone.Id));
    }
    
    public record struct CreateCardResult(string Name, Guid Card, Guid Zone);
    [Effect("CreateCard")]
    public static IEffectResult CreateCard(IResolutionContext context, string cardName, IZone zone)
    {
        var cardProto = context.GetState().GetCardPool().GetCard(cardName);
        
        if (cardProto is null)
        {
            return ResultFactory.NoOp();
        }
        
        var card = new Card(cardProto)
        {
            Zone = zone
        };
        
        return !zone.AddAtom(card) 
            ? ResultFactory.NoOp() 
            : ResultFactory.Atomic(new CreateCardResult(cardName, card.Id, zone.Id));
    }
    
    public record struct ResourceChangeResult(string ResourceType, int Change, int NewValue);
    
    [Effect("PayResource")]
    public static IEffectResult PayResource(IResolutionContext context, string resourceType, int amount)
    {
        if (amount <= 0)
        {
            return ResultFactory.NoOp();
        }
        
        var player = context.GetState().GetPlayer();
        
        var currentResource = player.GetResouceCount(resourceType) ?? 0;
        
        if (currentResource < amount)
        {
            return ResultFactory.NoOp();
        }
        
        var newValue = currentResource - amount;
        
        player.SetResourceCount(resourceType, newValue);
        
        return ResultFactory.Atomic(new ResourceChangeResult(resourceType, -amount, newValue));
    }
    
    [Effect("GainResource")]
    public static IEffectResult GainResource(IResolutionContext context, string resourceType, int amount)
    {
        if (amount <= 0)
        {
            return ResultFactory.NoOp();
        }
        
        var player = context.GetState().GetPlayer();
        
        var currentResource = player.GetResouceCount(resourceType) ?? 0;
        
        var newValue = currentResource + amount;
        
        player.SetResourceCount(resourceType, newValue);
        
        return ResultFactory.Atomic(new ResourceChangeResult(resourceType, amount, newValue));
    }
}