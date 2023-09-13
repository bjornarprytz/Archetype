using Archetype.Rules;
using Archetype.Rules.State;
using MediatR;

namespace Archetype.Runtime.Actions;

public class UseAbilityHandler : IRequestHandler<UseAbilityArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IEffectQueue _effectQueue;
    private readonly IGameState _gameState;
    private readonly Definitions _definitions;

    public UseAbilityHandler(IGameState gameState, Definitions definitions, IEffectQueue effectQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _effectQueue = effectQueue;
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

        _effectQueue.Push(resolutionContext);

        return Unit.Task;
    }
}