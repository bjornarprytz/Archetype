using Archetype.Rules;
using Archetype.Rules.State;
using MediatR;

namespace Archetype.Runtime.Actions;

public class UseAbilityHandler : IRequestHandler<UseAbilityArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IEffectQueue _effectQueue;
    private readonly GameState _gameState;
    private readonly Definitions _definitions;

    public UseAbilityHandler(GameState gameState, Definitions definitions, IEffectQueue effectQueue, IEventHistory history)
    {
        _gameState = gameState;
        _definitions = definitions;
        _effectQueue = effectQueue;
        _history = history;
    }

    public Task<Unit> Handle(UseAbilityArgs args, CancellationToken cancellationToken)
    {
        var protoAbility = args.Ability.Proto;
        var conditions = protoAbility.Conditions;
        var costs = protoAbility.Costs;
        var ability = args.Ability;
        var payments = args.Payments;
        
        ability.UpdateComputedValues(_definitions, _gameState);

        if (_definitions.CheckConditions(conditions, args.Ability.Source, _gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        // TODO: Check targets
        
        if (!_definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");

        foreach (var (cost, payment) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(_gameState, _definitions, payment));
        }
        
        var resolutionContext = ability.CreateResolutionContext(payments, args.Targets);

        _effectQueue.Push(resolutionContext);

        return Unit.Task;
    }
}