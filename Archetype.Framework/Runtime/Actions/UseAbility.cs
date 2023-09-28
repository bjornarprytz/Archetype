using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record UseAbilityArgs(Guid AbilitySource, string AbilityName, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;

public class UseAbilityHandler : IRequestHandler<UseAbilityArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IActionQueue _actionQueue;
    private readonly IGameRoot _gameRoot;
    private readonly IDefinitions _definitions;

    public UseAbilityHandler(IGameRoot gameRoot, IDefinitions definitions, IActionQueue actionQueue, IEventHistory history)
    {
        _gameRoot = gameRoot;
        _definitions = definitions;
        _actionQueue = actionQueue;
        _history = history;
    }

    public Task<Unit> Handle(UseAbilityArgs args, CancellationToken cancellationToken)
    {
        var gameState = _gameRoot.GameState;
        
        var abilitySource = gameState.GetAtom<ICard>(args.AbilitySource);
        var targets = args.Targets.Select(gameState.GetAtom).ToList();

        var ability = abilitySource.Abilities[args.AbilityName];
        var payments = args.Payments;
        var costs = ability.Costs;
        
        var resolutionContext = ability.CreateResolutionContext(_gameRoot, payments, targets);
        
        foreach (var (cost, payment, instance) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(gameState, _definitions, payment, instance));
        }

        _actionQueue.Push(new ResolutionFrame(resolutionContext, ability.Effects.ToList()));

        return Unit.Task;
    }
}