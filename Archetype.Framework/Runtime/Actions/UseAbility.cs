using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record UseAbilityArgs(Guid AbilitySource, int AbilityIndex, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;

public class UseAbilityHandler : IRequestHandler<UseAbilityArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IActionQueue _actionQueue;
    private readonly IGameState _gameState;
    private readonly IDefinitions _definitions;

    public UseAbilityHandler(IGameState gameState, IDefinitions definitions, IActionQueue actionQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _actionQueue = actionQueue;
        _history = history;
    }

    public Task<Unit> Handle(UseAbilityArgs args, CancellationToken cancellationToken)
    {
        var abilitySource = _gameState.GetAtom<ICard>(args.AbilitySource);
        var targets = args.Targets.Select(_gameState.GetAtom).ToList();

        var ability = abilitySource.Abilities[args.AbilityIndex];
        var protoAbility = ability.Proto;
        var conditions = protoAbility.Conditions;
        var costs = protoAbility.Costs;
        var payments = args.Payments;

        ability.UpdateComputedValues(_definitions, _gameState);

        if (_definitions.CheckConditions(conditions, abilitySource, _gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        if (abilitySource.GetTargetDescriptors().Zip(targets).All((t) => t.First.CheckTarget(t.Second)))
            throw new InvalidOperationException("Invalid targets");

        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }
        
        var resolutionContext = ability.CreateResolutionContext(payments, targets);

        _actionQueue.Push(resolutionContext);

        return Unit.Task;
    }
}