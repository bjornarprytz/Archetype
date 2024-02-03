using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record PaymentPayload(CostType Type, IReadOnlyList<Guid> Payment, int Amount);
public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<PaymentPayload> Payments) : IRequest<Unit>;

public class PlayCardHandler(IGameRoot gameRoot)
    : IRequestHandler<PlayCardArgs, Unit>
{
    public Task<Unit> Handle(PlayCardArgs args, CancellationToken cancellationToken)
    {
        var gameState = gameRoot.GameState;
        
        var card = gameState.GetAtom<ICard>(args.Card);
        
        card.ResolvePaymentsAndQueueEffects(gameRoot, args.Targets, args.Payments);

        return Unit.Task;
    }
}