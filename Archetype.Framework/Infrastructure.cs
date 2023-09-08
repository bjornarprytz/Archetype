namespace Archetype.Framework;

public interface IEventHistory
{
    public void Push(Event e);
    public IEnumerable<Event> Events { get; set; }
}

public interface IEffectQueue
{
    public void Push(EffectPayload payload);
    public IEnumerable<EffectPayload> Effects { get; }
    public bool ResolveNext();
}

public interface IAPI // TODO: This is probably better solved with mediator
{
    public void PlayCard(PlayCardPayload payload);
    public void UseAbility(AbilityPayload payload);
}

public class EffectQueue : IEffectQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly GameState _state;
    private readonly Definitions _definitions;
    
    private readonly Queue<EffectPayload> _queue = new();

    public EffectQueue(IEventHistory eventHistory, GameState state, Definitions definitions)
    {
        _eventHistory = eventHistory;
        _state = state;
        _definitions = definitions;
    }

    public void Push(EffectPayload payload)
    {
        _queue.Enqueue(payload);
    }

    public IEnumerable<EffectPayload> Effects => _queue;
    public bool ResolveNext()
    {
        if (_queue.Count == 0)
            return false;
        
        var payload = _queue.Dequeue();
        var e = Resolve(payload);
        _eventHistory.Push(e);
        
        return true;
    }
    
    private Event Resolve(EffectPayload payload)
    {
        if (_definitions.Keywords[payload.Keyword] is not EffectDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");

        return effectDefinition.Resolve(_state, _definitions, payload);
    }
}

public class CardHandler : IAPI
{
    private readonly IEventHistory _history;
    private readonly IEffectQueue _effectQueue;
    private readonly GameState _gameState;
    private readonly Definitions _definitions;

    public CardHandler(IEventHistory history, IEffectQueue effectQueue, GameState gameState, Definitions definitions)
    {
        _history = history;
        _effectQueue = effectQueue;
        _gameState = gameState;
        _definitions = definitions;
    }
    
    public void PlayCard(PlayCardPayload payload)
    {
        var conditions = payload.Card.Proto.Conditions;
        var costs = payload.Card.Proto.Costs;
        var payments = payload.Payments;
        
        if (!conditions.All(c => c.Check(payload.Card, _gameState)))
            throw new InvalidOperationException("Invalid conditions");
        
        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }

        foreach (var effect in payload.Card.Proto.CreateEffects(payload.CardArgs))
        {
            _effectQueue.Push(effect);
        }
    }
    
    public void UseAbility(AbilityPayload payload)
    {
        var ability = payload.Card.Proto.Abilities.ElementAt(payload.AbilityIndex);
        var conditions = ability.Conditions;
        var costs = ability.Costs;
        var payments = payload.Payments;
        
        if (!conditions.All(c => c.Check(payload.Card, _gameState)))
            throw new InvalidOperationException("Invalid conditions");

        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }

        foreach (var effect in ability.CreateEffects(payload.AbilityArgs))
        {
            _effectQueue.Push(effect);
        };
    }
}
