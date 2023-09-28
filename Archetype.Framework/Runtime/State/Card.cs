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
        
        Abilities = proto.Abilities.ToDictionary(
            pair => pair.Key,
            pair => new Ability
            {
                Proto = pair.Value,
                Source = this
            } as IAbility
        );
    }
    
    public IReadOnlyDictionary<string, IAbility> Abilities { get; }
    
    public IZone? CurrentZone { get; set; }
    public bool Tapped { get; set; }
    public IAtom Source => this;
    public IReadOnlyList<TargetDescription> TargetsDescriptors => _proto.Targets;
    public IReadOnlyList<KeywordInstance> Effects => _proto.Effects;
    public IReadOnlyList<KeywordInstance> Costs => _proto.Costs;
    public IReadOnlyList<KeywordInstance> Conditions => _proto.Conditions;
    public IReadOnlyList<object> ComputedValues => _computedValues;
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics => _proto.Characteristics;
    

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
        
    public Guid Id { get; }
    public ProtoAbility Proto { get; init; }
    public IAtom Source { get; init; }
    public IReadOnlyList<TargetDescription> TargetsDescriptors => Proto.Targets;
    public IReadOnlyList<KeywordInstance> Effects => Proto.Effects;
    public IReadOnlyList<KeywordInstance> Costs => Proto.Costs;
    public IReadOnlyList<KeywordInstance> Conditions => Proto.Conditions;
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
}
