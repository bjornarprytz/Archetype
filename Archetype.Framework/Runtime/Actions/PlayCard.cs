using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler : IRequestHandler<PlayCardArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IEffectQueue _effectQueue;
    private readonly IGameState _gameState;
    private readonly IDefinitions _definitions;

    public PlayCardHandler(IGameState gameState, IDefinitions definitions, IEffectQueue effectQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _effectQueue = effectQueue;
        _history = history;
    }

    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var card = _gameState.GetAtom<ICard>(args.Card);
        var targets = args.Targets.Select(_gameState.GetAtom).ToList();
        
        var conditions = card.Proto.Conditions;
        var costs = card.Proto.Costs;
        var payments = args.Payments;
        
        card.UpdateComputedValues(_definitions, _gameState);
        
        if (_definitions.CheckConditions(conditions, card, _gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        if (!card.CheckTargets(targets))
            throw new InvalidOperationException("Invalid targets");

        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }

        var resolutionContext = card.CreateResolutionContext(payments, targets);
        
        _effectQueue.Push(resolutionContext);

        return Unit.Task;
    }
}