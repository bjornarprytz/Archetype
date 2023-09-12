using Archetype.Rules;
using Archetype.Rules.State;
using MediatR;

namespace Archetype.Runtime.Actions;

public class PlayCardHandler : IRequestHandler<PlayCardArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IEffectQueue _effectQueue;
    private readonly GameState _gameState;
    private readonly Definitions _definitions;

    public PlayCardHandler(GameState gameState, Definitions definitions, IEffectQueue effectQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _effectQueue = effectQueue;
        _history = history;
    }

    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var conditions = args.Card.Proto.Conditions;
        var costs = args.Card.Proto.Costs;
        var payments = args.Payments;
        var targets = args.Targets;
        
        args.Card.UpdateComputedValues(_definitions, _gameState);
        
        if (_definitions.CheckConditions(conditions, args.Card, _gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        // TODO: Check targets

        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }

        var resolutionContext = args.Card.CreateResolutionContext(payments, targets);
        
        _effectQueue.Push(resolutionContext);

        return Unit.Task;
    }
}