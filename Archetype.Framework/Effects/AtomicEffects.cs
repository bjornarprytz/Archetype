using Archetype.Framework.Core;
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
        
        target.SetStat(stat, value);
        
        return ResultFactory.Atomic(value - currentValue);
    }
    
    [Effect("AddTag")]
    public static IEffectResult AddTag(IHasTags target, string tag)
    {
        if (tag is not { Length: > 0 } || target.HasTag(tag))
        {
            return ResultFactory.NoOp();
        }
        
        target.AddTag(tag);
        
        return ResultFactory.Atomic(tag);
    }
    
    [Effect("RemoveTag")]
    public static IEffectResult RemoveTag(IHasTags target, string tag)
    {
        if (tag is not { Length: > 0 } || !target.HasTag(tag))
        {
            return ResultFactory.NoOp();
        }
        
        target.RemoveTag(tag);
        
        return ResultFactory.Atomic(tag);
    }
    
    [Effect("SetFacet")]
    public static IEffectResult SetFacet(IHasFacets target, string key, string[] value)
    {
        if (key is not { Length: > 0 } || value is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        target.SetFacet(key, value);
        
        return ResultFactory.Atomic(value);
    }
    
    [Effect("ClearFacet")]
    public static IEffectResult ClearFacet(IHasFacets target, string key)
    {
        if (key is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var value = target.GetFacet(key);
        
        if (value is null)
        {
            return ResultFactory.NoOp();
        }
        
        target.RemoveFacet(key);
        
        return ResultFactory.Atomic(value);
    }
    
    [Effect("RemoveFacets")]
    public static IEffectResult RemoveFacets(IHasFacets target, string key, string[] valuesToRemove)
    {
        if (key is not { Length: > 0 } || valuesToRemove is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var values = target.GetFacet(key);
        
        if (values is null)
        {
            return ResultFactory.NoOp();
        }
        
        var newValue = values.Except(valuesToRemove).ToArray();
        
        target.SetFacet(key, newValue);
        
        return ResultFactory.Atomic(valuesToRemove);
    }
    
    [Effect("AddFacets")]
    public static IEffectResult AddFacets(IHasFacets target, string key, string[] valuesToAdd)
    {
        if (key is not { Length: > 0 } || valuesToAdd is not { Length: > 0 })
        {
            return ResultFactory.NoOp();
        }
        
        var currentValues = target.GetFacet(key);
        
        if (currentValues is null)
        {
            return ResultFactory.NoOp();
        }
        
        var addedValues = valuesToAdd.Except(currentValues).ToArray();
        
        var newValue = currentValues.Concat(valuesToAdd).Distinct().ToArray();
        
        target.SetFacet(key, newValue);
        
        return ResultFactory.Atomic(addedValues);
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
    public static IEffectResult CreateCard(CardProto proto, IZone zone)
    {
        var card = new Card(proto)
        {
            Zone = zone
        };
        
        return !zone.AddAtom(card) 
            ? ResultFactory.NoOp() 
            : ResultFactory.Atomic(new CreateCardResult(proto.Name, card.Id, zone.Id));
    }
}