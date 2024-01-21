using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.State;
using FluentValidation;

namespace Archetype.Framework.DependencyInjection;

public class ProtoCardBuilder
{
    private string? _name;
    
    private IProtoActionBlock? _actionBlock;
    private readonly Dictionary<string, IProtoActionBlock> _abilities = new();
    private readonly Dictionary<string, IKeywordInstance> _characteristics = new();
    
    public IProtoCard Build()
    {
        if (_name is null)
        {
            throw new InvalidOperationException("Name must be set before building");
        }
        
        if (_actionBlock is null)
        {
            throw new InvalidOperationException("Action block must be set before building");
        }
        
        var protoCard = new ProtoCard (
            Name: _name,
            ActionBlock: _actionBlock,
            Abilities: _abilities,
            Characteristics: _characteristics
        );
        
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
            _characteristics.Add(instance.ResolveFuncName, instance);
        }
    }
    
    public void AddAbility(string name, Action<ActionBlockBuilder> buildAction)
    {
        var builder = new ActionBlockBuilder();
        buildAction(builder);
        
        _abilities.Add(name, builder.Build());
    }

    public void BuildActionBlock(
        Action<ActionBlockBuilder> buildAction
        )
    {
        var builder = new ActionBlockBuilder();
        buildAction(builder);
        
        _actionBlock = builder.Build();
    }
    
    public class ActionBlockBuilder
    {
        private readonly List<IKeywordInstance> _targetSpecs = new();
        private readonly List<IKeywordInstance> _conditions = new();
        private readonly List<IKeywordInstance> _effects = new();
        private readonly List<IKeywordInstance> _costs = new();
        private readonly List<IKeywordInstance> _computedValues = new();
        
        public void AddTargetSpecs(List<IKeywordInstance> targetSpecs)
        {
            _targetSpecs.AddRange(targetSpecs);
        }
        
        public void AddConditions(List<IKeywordInstance> conditions)
        {
            _conditions.AddRange(conditions);
        }
        
        public void AddEffects(List<IKeywordInstance> effects)
        {
            _effects.AddRange(effects);
        }
        
        public void AddCosts(List<IKeywordInstance> costs)
        {
            _costs.AddRange(costs);
        }
        
        public void AddComputedValues(List<IKeywordInstance> computedValues)
        {
            _computedValues.AddRange(computedValues);
        }
        
        
        public IProtoActionBlock Build()
        {
            return new ProtoActionBlock(
                TargetSpecs: _targetSpecs,
                Conditions: _conditions,
                Costs: _costs,
                Effects: _effects,
                AfterEffects: Array.Empty<IKeywordInstance>(),
                ComputedValues: _computedValues
            );
        }
    }

    public record ProtoCard(
        string Name, 
        IProtoActionBlock ActionBlock,
        IReadOnlyDictionary<string, IProtoActionBlock> Abilities, 
        IReadOnlyDictionary<string, IKeywordInstance> Characteristics
        ) : IProtoCard;

    public record ProtoActionBlock(
        IReadOnlyList<IKeywordInstance> TargetSpecs,
        IReadOnlyList<IKeywordInstance> Conditions,
        IReadOnlyList<IKeywordInstance> Costs,
        IReadOnlyList<IKeywordInstance> Effects,
        IReadOnlyList<IKeywordInstance> AfterEffects,
        IReadOnlyList<IKeywordInstance> ComputedValues
    ) : IProtoActionBlock;
}