using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record PaymentPayload(CostType Type, IReadOnlyList<IAtom> Payment, int Amount);
public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<PaymentPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler(IGameRoot gameRoot, IActionQueue actionQueue)
    : IRequestHandler<PlayCardArgs, Unit>
{
    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var gameState = gameRoot.GameState;
        
        var card = gameState.GetAtom<ICard>(args.Card);
        var targets = args.Targets.Select(gameState.GetAtom).ToList();
        
        var costs = card.Costs;
        var payments = args.Payments;
        var effects = card.Effects.Concat(card.AfterEffects).ToList();
        
        var resolutionContext = card.CreateAndValidateResolutionContext(gameRoot.GameState, gameRoot.MetaGameState, payments, targets);

        actionQueue.Push(new ResolutionFrame(resolutionContext, costs, effects));

        return Unit.Task;
    }
}