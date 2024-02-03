using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record UseAbilityArgs(Guid AbilitySource, string AbilityName, IReadOnlyList<Guid> Targets, IReadOnlyList<PaymentPayload> Payments) : IRequest<Unit>;

public class UseAbilityHandler(IGameRoot gameRoot)
    : IRequestHandler<UseAbilityArgs, Unit>
{
    public Task<Unit> Handle(UseAbilityArgs args, CancellationToken cancellationToken)
    {
        var gameState = gameRoot.GameState;
        
        var abilitySource = gameState.GetAtom<ICard>(args.AbilitySource);

        var ability = abilitySource.Abilities[args.AbilityName];
        
        ability.ResolvePaymentsAndQueueEffects(gameRoot, args.Targets, args.Payments);

        return Unit.Task;
    }
}