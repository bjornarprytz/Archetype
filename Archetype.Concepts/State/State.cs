using Archetype.Rules.Definitions;
using Archetype.Rules.Proto;

namespace Archetype.Rules.State;

public class Definitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}

public interface IAtom
{
    Guid Id { get; set; }
    IDictionary<string, string> Characteristics { get; set; }
}

public interface IZone : IAtom
{
    IEnumerable<ICard> Cards { get; set; }
}

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; set; }
    IDictionary<Guid, IAtom> Atoms { get; set; }
}

public interface IActionBlock
{
    public IAtom Source { get; }
    public IReadOnlyList<EffectInstance> Effects { get; }
    public IReadOnlyList<CostInstance> Costs { get; }

    public object? GetComputedValue(string key);
    void UpdateComputedValues(State.Definitions definitions, IGameState gameState);
    
    IEnumerable<TargetDescription> GetTargetDescriptors();
}

public interface ICard : IAtom, IActionBlock
{
    IZone CurrentZone { get; set; }
    ProtoCard Proto { get; set; }
    IReadOnlyList<IAbility> Abilities { get; set; }
}

public interface IAbility : IActionBlock
{
    public AbilityInstance Proto { get; set; }
}

public class Card : ICard
{
    public Guid Id { get; set; }
    public IAtom Source => this;
    public IZone CurrentZone { get; set; }
    public ProtoCard Proto { get; set; }
    public object Modifiers { get; set; } // TODO: Define this
    public object RulesText { get; set; } // TODO: Define this
    
    public IDictionary<string, string> Characteristics { get; set; } // TODO: Define this, with proto and modifiers in mind
    private IDictionary<string, object> _computedValues { get; set; } // Use this to create rules text

    public IReadOnlyList<IAbility> Abilities { get; set; }
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;

    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(Definitions definitions, IGameState gameState)
    {
        var computedValues = Proto.ComputedValues;

        foreach (var computedProperty in computedValues)
        {
            var definition = definitions.GetOrThrow<ComputedValueDefinition>(computedProperty);
            
            var value = definition.Compute(this, gameState);
            
            if (value is not int or string)
                throw new InvalidOperationException($"Computed property ({computedProperty.Key}) is not an int or string");
            
            _computedValues[computedProperty.Key] = value;
        }
    }

    public IEnumerable<TargetDescription> GetTargetDescriptors()
    {
        return Proto.Effects.SelectMany(e => e.Targets).DistinctBy(t => t.Index).OrderBy(t => t.Index);
    }
}

public class Ability : IAbility
{
    public IAtom Source { get; set; }
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;
    
    public AbilityInstance Proto { get; set; }
    
    private IDictionary<string, object> _computedValues { get; set; } // Use this to create rules text

    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(Definitions definitions, IGameState gameState)
    {
        var computedValues = Proto.ComputedValues;

        foreach (var computedProperty in computedValues)
        {
            var definition = definitions.GetOrThrow<ComputedValueDefinition>(computedProperty);
            
            var value = definition.Compute(Source, gameState);
            
            if (value is not int or string)
                throw new InvalidOperationException($"Computed property ({computedProperty.Key}) is not an int or string");
            
            _computedValues[computedProperty.Key] = value;
        }
    }
    
    public IEnumerable<TargetDescription> GetTargetDescriptors()
    {
        return Proto.Effects.SelectMany(e => e.Targets).DistinctBy(t => t.Index).OrderBy(t => t.Index);
    }
}