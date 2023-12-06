using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record UseAbilityArgs(Guid AbilitySource, string AbilityName, IReadOnlyList<Guid> Targets, IReadOnlyList<PaymentPayload> Payments) : IRequest<Unit>;

public class UseAbilityHandler(IGameRoot gameRoot, IActionQueue actionQueue)
    : IRequestHandler<UseAbilityArgs, Unit>
{
    public Task<Unit> Handle(UseAbilityArgs args, CancellationToken cancellationToken)
    {
        var gameState = gameRoot.GameState;
        
        var abilitySource = gameState.GetAtom<ICard>(args.AbilitySource);
        var targets = args.Targets.Select(gameState.GetAtom).ToList();

        var ability = abilitySource.Abilities[args.AbilityName];
        var payments = args.Payments;
        var costs = ability.Costs;
        var effects = ability.Effects.Concat(ability.AfterEffects).ToList();
        
        var resolutionContext = ability.CreateAndValidateResolutionContext(gameRoot.GameState, gameRoot.MetaGameState, payments, targets);

        actionQueue.Push(new ResolutionFrame(resolutionContext, costs, effects));

        return Unit.Task;
    }
}