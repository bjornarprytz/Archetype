﻿using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record CostPayload(CostType Type, IReadOnlyList<ICard> Payment);
public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler : IRequestHandler<PlayCardArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IActionQueue _actionQueue;
    private readonly IGameState _gameState;
    private readonly IDefinitions _definitions;

    public PlayCardHandler(IGameState gameState, IDefinitions definitions, IActionQueue actionQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _actionQueue = actionQueue;
        _history = history;
    }

    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var card = _gameState.GetAtom<ICard>(args.Card);
        var targets = args.Targets.Select(_gameState.GetAtom).ToList();
        
        var conditions = card.Conditions;
        var costs = card.Costs;
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
        
        _actionQueue.Push(resolutionContext);

        return Unit.Task;
    }
}