using Archetype.Rules.Definitions;
using Archetype.Rules.Proto;

namespace Archetype.Rules.State;

public class Definitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}

public abstract class Atom
{
    public Guid Id { get; set; }
}

public abstract class Zone : Atom
{
    public IEnumerable<Card> Cards { get; set; }
}

public class GameState
{
    
}

public interface IComputedValuesCache
{
    object? GetComputedValue(string key);
    void UpdateComputedValues(Definitions definitions, GameState gameState);
}

public class Card : Atom, IActionBlock
{
    public Zone CurrentZone { get; set; }
    public ProtoCard Proto { get; set; }
    public object Modifiers { get; set; } // TODO: Define this
    public object RulesText { get; set; } // TODO: Define this
    
    private IDictionary<string, object> _computedValues { get; set; } // Use this to create rules text

    public Card Source => this;
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;

    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(Definitions definitions, GameState gameState)
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
}

public class Ability : IActionBlock
{
    public Card Source { get; set; }
    public IReadOnlyList<EffectInstance> Effects => Proto.Effects;
    public IReadOnlyList<CostInstance> Costs => Proto.Costs;
    
    public AbilityInstance Proto { get; set; }
    
    private IDictionary<string, object> _computedValues { get; set; } // Use this to create rules text

    public object? GetComputedValue(string key)
    {
        return _computedValues.TryGetValue(key, out var value) ? value : null;
    }

    public void UpdateComputedValues(Definitions definitions, GameState gameState)
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
}