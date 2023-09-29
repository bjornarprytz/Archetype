using Archetype.Framework.Definitions;

namespace Archetype.Framework.Proto;

public class ProtoBuilder
{
    private string _name;
    private readonly List<CardTargetDescription> _targetSpecs = new();
    private readonly List<KeywordInstance> _conditions = new();
    private readonly List<KeywordInstance> _effects = new();
    private readonly List<KeywordInstance> _costs = new();
    private readonly List<KeywordInstance> _computedValues = new();
    private readonly Dictionary<string, IProtoActionBlock> _abilities = new();
    private readonly Dictionary<string, KeywordInstance> _characteristics = new();

    public ProtoBuilder()
    {
        
    }
    
    
    public IProtoCard Build()
    {
        // TODO: Validate card
        // TODO: Add type specific effects and conditions
        // e.g. units always target nodes, and put themselves into play
        // spells go to the discard pile
        
        // NOTE: This could be a pluggable behaviour, because it's encroaching on the game design domain
        
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
        
        return protoCard;
    }
    
    public void SetName(string name)
    {
        _name = name;
    }
    
    public void AddCharacteristics(List<KeywordInstance> characteristicsInstances)
    {
        foreach (var instance in characteristicsInstances)
        {
            _characteristics.Add(instance.Keyword, instance);
        }
    }
    

    public void SetActionBlock(
        List<CardTargetDescription> targetSpecs, 
        List<KeywordInstance> costs, 
        List<KeywordInstance> conditions, 
        List<KeywordInstance> computedValues, 
        List<KeywordInstance> effects
        )
    {
        _targetSpecs.AddRange(targetSpecs);
        _costs.AddRange(costs);
        _conditions.AddRange(conditions);
        _computedValues.AddRange(computedValues);
        _effects.AddRange(effects);
    }

    private record ProtoCard(
        string Name, 
        IProtoActionBlock ActionBlock,
        IReadOnlyDictionary<string, IProtoActionBlock> Abilities, 
        IReadOnlyDictionary<string, KeywordInstance> Characteristics
        ) : IProtoCard;

    private record ProtoActionBlock(
        IReadOnlyList<CardTargetDescription> TargetSpecs,
        IReadOnlyList<KeywordInstance> Conditions,
        IReadOnlyList<KeywordInstance> Costs,
        IReadOnlyList<KeywordInstance> Effects,
        IReadOnlyList<KeywordInstance> ComputedValues
    ) : IProtoActionBlock;
}