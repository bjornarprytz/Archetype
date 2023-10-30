using Archetype.Framework.Definitions;
using FluentValidation;

namespace Archetype.Framework.Proto;

public class ProtoBuilder
{
    private readonly IEnumerable<IValidator<IProtoCard>> _validators;
    private readonly IList<Action<ProtoCard>> _onBuildPlugin = new List<Action<ProtoCard>>();
    
    private string? _name;
    private readonly List<CardTargetDescription> _targetSpecs = new();
    private readonly List<IKeywordInstance> _conditions = new();
    private readonly List<IKeywordInstance> _effects = new();
    private readonly List<IKeywordInstance> _costs = new();
    private readonly List<IKeywordInstance> _computedValues = new();
    private readonly Dictionary<string, IProtoActionBlock> _abilities = new();
    private readonly Dictionary<string, IKeywordInstance> _characteristics = new();

    public ProtoBuilder(IEnumerable<IValidator<IProtoCard>> validators, IEnumerable<Action<ProtoCard>> plugins)
    {
        _validators = validators;
        foreach (var plugin in plugins)
        {
            _onBuildPlugin.Add(plugin);
        }
    }
    
    public IProtoCard Build()
    {
        var protoCard = new ProtoCard (
            Name: _name,
            ActionBlock: new ProtoActionBlock(
                TargetSpecs: _targetSpecs,
                Conditions: _conditions,
                Costs: _costs,
                Effects: _effects,
                ComputedValues: _computedValues
            ),
            Abilities: _abilities,
            Characteristics: _characteristics
        );
        
        foreach (var plugin in _onBuildPlugin)
        {
            plugin(protoCard);
        }
        
        foreach (var validator in _validators)
        {
            validator.ValidateAndThrow(protoCard);
        }
        
        return protoCard;
    }
    
    public void SetName(string name)
    {
        _name = name;
    }
    
    public void AddCharacteristics(List<IKeywordInstance> characteristicsInstances)
    {
        foreach (var instance in characteristicsInstances)
        {
            _characteristics.Add(instance.Keyword, instance);
        }
    }
    

    public void SetActionBlock(
        List<CardTargetDescription> targetSpecs, 
        List<IKeywordInstance> costs, 
        List<IKeywordInstance> conditions, 
        List<IKeywordInstance> computedValues, 
        List<IKeywordInstance> effects
        )
    {
        _targetSpecs.AddRange(targetSpecs);
        _costs.AddRange(costs);
        _conditions.AddRange(conditions);
        _computedValues.AddRange(computedValues);
        _effects.AddRange(effects);
    }

    public record ProtoCard(
        string Name, 
        IProtoActionBlock ActionBlock,
        IReadOnlyDictionary<string, IProtoActionBlock> Abilities, 
        IReadOnlyDictionary<string, IKeywordInstance> Characteristics
        ) : IProtoCard;

    public record ProtoActionBlock(
        IReadOnlyList<CardTargetDescription> TargetSpecs,
        IReadOnlyList<IKeywordInstance> Conditions,
        IReadOnlyList<IKeywordInstance> Costs,
        IReadOnlyList<IKeywordInstance> Effects,
        IReadOnlyList<IKeywordInstance> ComputedValues
    ) : IProtoActionBlock;
}