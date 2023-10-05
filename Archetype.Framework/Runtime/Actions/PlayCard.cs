using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record PaymentPayload(CostType Type, IReadOnlyList<IAtom> Payment);
public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<PaymentPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler : IRequestHandler<PlayCardArgs, Unit>
{
    private readonly IEventHistory _history;
    private readonly IActionQueue _actionQueue;
    private readonly IGameRoot _gameRoot;
    private readonly IRules _rules;

    public PlayCardHandler(IGameRoot gameRoot, IRules rules, IActionQueue actionQueue, IEventHistory history)
    {
        _gameRoot = gameRoot;
        _rules = rules;
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
        var effects = card.Effects;
        
        var resolutionContext = card.CreateAndValidateResolutionContext(_gameRoot, payments, targets);

        _actionQueue.Push(new ResolutionFrame(resolutionContext, costs, effects));

        return Unit.Task;
    }
}