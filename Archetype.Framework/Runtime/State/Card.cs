﻿using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public class Card : ICard
{
    private readonly Dictionary<string, object> _computedValues = new(); 
        
    public Guid Id { get; set; }
    public ProtoCard Proto { get; init; }
    public IReadOnlyList<IAbility> Abilities { get; init; }
    
    public IZone CurrentZone { get; set; }
    public IAtom Source => this;
    
    public IReadOnlyList<FeatureInstance> Features => Proto.Features;
    public IReadOnlyList<ReactionInstance> Reactions => Proto.Reactions;
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;
    public IReadOnlyList<ConditionInstance> Conditions => Proto.Conditions;
    public IReadOnlyDictionary<string, string> Characteristics => Proto.Characteristics;
    


    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(IDefinitions definitions, IGameState gameState)
    {
        foreach (var computedValue in Proto.ComputedValues)
        {
            var keywordDefinition = definitions.GetOrThrow<ComputedValueDefinition>(computedValue);
            
            _computedValues[computedValue.Key] = keywordDefinition.Compute(Source, gameState);
        }
    }

    public IEnumerable<TargetDescription> GetTargetDescriptors()
    {
        return Effects.SelectMany(e => e.Targets).DistinctBy(t => t.Index).OrderBy(t => t.Index);
    }
}

public class Ability : IAbility
{
        
    public Guid Id { get; init; }
    public AbilityInstance Proto { get; init; }
    public IAtom Source { get; init; }
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;
    public IReadOnlyList<ConditionInstance> Conditions => Proto.Conditions;

    private readonly Dictionary<string, object> _computedValues = new(); 
    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(IDefinitions definitions, IGameState gameState)
    {
        foreach (var computedValue in Proto.ComputedValues)
        {
            var keywordDefinition = definitions.GetOrThrow<ComputedValueDefinition>(computedValue);
            
            _computedValues[computedValue.Key] = keywordDefinition.Compute(Source, gameState);
        }
    }

    public IEnumerable<TargetDescription> GetTargetDescriptors()
    {
        return Effects.SelectMany(e => e.Targets).DistinctBy(t => t.Index).OrderBy(t => t.Index);
    }

    public IReadOnlyList<FeatureInstance> Features { get; }
}