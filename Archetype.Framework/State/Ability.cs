using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public interface IAbility : IActionBlock { }

public class Ability : IAbility
{
    private readonly List<int> _computedValues = new(); 
    
    public required IProtoActionBlock Proto { get; init; }
    public required IAtom Source { get; init; }
    public IReadOnlyList<CardTargetDescription> TargetsDescriptors => Proto.TargetSpecs;
    public IReadOnlyList<IKeywordInstance> Effects => Proto.Effects;
    public IReadOnlyList<IKeywordInstance> AfterEffects => Proto.AfterEffects;
    public IReadOnlyList<IKeywordInstance> Costs => Proto.Costs;
    public IReadOnlyList<IKeywordInstance> Conditions => Proto.Conditions;
    public IReadOnlyList<int> ComputedValues => _computedValues;


    public object? GetComputedValue(int index)
    {
        return _computedValues.Count <= index ? null : _computedValues[index];
    }

    public void UpdateComputedValues(IRules rules, IResolutionContext resolutionContext)
    {
        foreach (var (computedValue, index) in Proto.ComputedValues.Select(((instance, i) => (instance, i))))
        {
            var keywordDefinition = rules.GetOrThrow<ComputedValueDefinition>(computedValue);
            
            _computedValues[index] = keywordDefinition.Compute(resolutionContext, computedValue);
        }
    }
}