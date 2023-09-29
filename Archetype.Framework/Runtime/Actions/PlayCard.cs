using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record CostPayload(CostType Type, IReadOnlyList<ICard> Payment);
public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler : IRequestHandler<PlayCardArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IActionQueue _actionQueue;
    private readonly IGameRoot _gameRoot;
    private readonly IDefinitions _definitions;

    public PlayCardHandler(IGameRoot gameRoot, IDefinitions definitions, IActionQueue actionQueue, IEventHistory history)
    {
        _gameRoot = gameRoot;
        _definitions = definitions;
        _actionQueue = actionQueue;
        _history = history;
    }

    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var gameState = _gameRoot.GameState;
        
        var card = gameState.GetAtom<ICard>(args.Card);
        var targets = args.Targets.Select(gameState.GetAtom).ToList();
        
        var costs = card.Costs;
        var payments = args.Payments;
        
        var resolutionContext = card.CreateAndValidateResolutionContext(_gameRoot, payments, targets);

        foreach (var (cost, payment, instance) in _definitions.EnumerateCosts(costs, payments))
        {
            _history.Push(cost.Resolve(gameState, _definitions, payment, instance));
        }

        _actionQueue.Push(new ResolutionFrame(resolutionContext, card.Effects.ToList()));

        return Unit.Task;
    }
}