using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public class Card : Atom, ICard
{
    private readonly List<object> _computedValues = new();
    private readonly ProtoCard _proto;

    public Card(ProtoCard proto)
    {
        _proto = proto;
    }
    
    public IReadOnlyDictionary<string, IAbility> Abilities { get; init; }
    
    public IZone? CurrentZone { get; set; }
    public bool Tapped { get; set; }
    public IAtom Source => this;
    public IReadOnlyList<TargetDescription> TargetsDescriptors => _proto.Targets;
    public IReadOnlyList<FeatureInstance> Features => _proto.Features;
    public IReadOnlyList<ReactionInstance> Reactions => _proto.Reactions;
    public IReadOnlyList<EffectInstance> Effects => _proto.Effects;
    public IReadOnlyList<CostInstance> Costs => _proto.Costs;
    public IReadOnlyList<ConditionInstance> Conditions => _proto.Conditions;
    public IReadOnlyList<object> ComputedValues => _computedValues;
    public override IReadOnlyDictionary<string, string> Characteristics => _proto.Characteristics;
    

    public void UpdateComputedValues(IDefinitions definitions, IGameState gameState)
    {
        foreach (var (computedValue, index) in _proto.ComputedValues.Select(((instance, i) => (instance, i))))
        {
            var keywordDefinition = definitions.GetOrThrow<ComputedValueDefinition>(computedValue);
            
            _computedValues[index] = keywordDefinition.Compute(Source, gameState);
        }
    }
}

public class Ability : IAbility
{
    private readonly List<object> _computedValues = new(); 
        
    public Guid Id { get; init; }
    public AbilityInstance Proto { get; init; }
    public IAtom Source { get; init; }
    public IReadOnlyList<TargetDescription> TargetsDescriptors { get; }
    public IReadOnlyList<TargetDescription> Targets => Proto.Targets;
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;
    public IReadOnlyList<ConditionInstance> Conditions => Proto.Conditions;
    public IReadOnlyList<object> ComputedValues => _computedValues;


    public object? GetComputedValue(int index)
    {
        return _computedValues.Count <= index ? null : _computedValues[index];
    }

    public void UpdateComputedValues(IDefinitions definitions, IGameState gameState)
    {
        foreach (var (computedValue, index) in Proto.ComputedValues.Select(((instance, i) => (instance, i))))
        {
            var keywordDefinition = definitions.GetOrThrow<ComputedValueDefinition>(computedValue);
            
            _computedValues[index] = keywordDefinition.Compute(Source, gameState);
        }
    }

    public IReadOnlyList<FeatureInstance> Features { get; }
}
