namespace Archetype.Framework;

public interface IEventHistory
{
    public void Push(Event e);
    public IEnumerable<Event> Events { get; set; }
}

public class EffectHandler
{
    public Event Resolve(GameState state, Definitions definitions, EffectPayload payload)
    {
        // Lookup the keyword definition
        var keywordDefinition = definitions.Keywords[payload.Keyword];
        
        if (keywordDefinition is not EffectDefinition effectDefinition)
            throw new InvalidOperationException("Keyword is not an effect");

        return effectDefinition.Resolve(state, definitions, payload);
    }
}

public class CardHandler
{
    private readonly IEventHistory _history;
    private readonly GameState _gameState;
    private readonly Definitions _definitions;

    public CardHandler(IEventHistory history, GameState gameState, Definitions definitions)
    {
        _history = history;
        _gameState = gameState;
        _definitions = definitions;
    }
    
    public IEnumerable<EffectPayload> PlayCard(PlayCardPayload payload)
    {
        var conditions = payload.Card.Proto.Conditions;
        
        if (!conditions.All(c => c.Check(payload.Card, _gameState)))
            throw new InvalidOperationException("Invalid conditions");
        
        var cost = payload.Card.Proto.Cost; 
        
        if (!cost.Definition.Check(payload.Payment))
            throw new InvalidOperationException("Invalid payment");

        var costEvent = cost.Definition.Resolve(_gameState, _definitions, payload.Payment); 
        _history.Push(costEvent);
        
        return payload.Card.Proto.CreateEffects(payload.CardArgs);
    }
}

public class AbilityHandler
{
    private readonly IEventHistory _history;
    private readonly GameState _gameState;
    private readonly Definitions _definitions;

    public AbilityHandler(IEventHistory history, GameState gameState, Definitions definitions)
    {
        _history = history;
        _gameState = gameState;
        _definitions = definitions;
    }
    
    public IEnumerable<EffectPayload> UseEffect(AbilityPayload payload)
    {
        var ability = payload.Card.Proto.Abilities.ElementAt(payload.AbilityIndex);
        
        var conditions = ability.Conditions;
        
        if (!conditions.All(c => c.Check(payload.Card, _gameState)))
            throw new InvalidOperationException("Invalid conditions");

        var cost = ability.Cost; 
        
        if (!cost.Definition.Check(payload.Payment))
            throw new InvalidOperationException("Invalid payment");

        var costEvent = cost.Definition.Resolve(_gameState, _definitions, payload.Payment); 
        _history.Push(costEvent);
        
        return ability.CreateEffects(payload.AbilityArgs);
    }
}