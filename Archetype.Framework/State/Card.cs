using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public interface ICard : IAtom, IActionBlock
{
    string Name { get; }
    IReadOnlyDictionary<string, IAbility> Abilities { get; }
}

public class Card : Atom, ICard
{
    private readonly List<int> _computedValues = new();
    private readonly IProtoCard _proto;

    public Card(IProtoCard proto)
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

    public string Name => _proto.Name;
    public IReadOnlyDictionary<string, IAbility> Abilities { get; }
    
    public IAtom Source => this;
    public IReadOnlyList<IKeywordInstance> TargetsDescriptors => _proto.ActionBlock.TargetSpecs;
    public IReadOnlyList<IKeywordInstance> Effects => _proto.ActionBlock.Effects;
    public IReadOnlyList<IKeywordInstance> AfterEffects => _proto.ActionBlock.AfterEffects;
    public IReadOnlyList<IKeywordInstance> Costs => _proto.ActionBlock.Costs;
    public IReadOnlyList<int> ComputedValues => _computedValues;
    public override IReadOnlyDictionary<string, int> Stats => _proto.Stats;
    public override IReadOnlyDictionary<string, string> Tags => _proto.Tags;
    

    public void UpdateComputedValues(IRules rules, IResolutionContext resolutionContext)
    {
        foreach (var (computedValue, index) in _proto.ActionBlock.ComputedValues.Select((instance, i) => (instance, i)))
        {
            var keywordDefinition = rules.GetOrThrow<ComputeDefinition>(computedValue);
            
            _computedValues[index] = keywordDefinition.Compute(resolutionContext, computedValue);
        }
    }

}


