using Archetype.Framework.Core.Structure;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record PassTurnArgs() : IRequest<Unit>;

public class PassTurnHandler(IActionQueue actionQueue) : IRequestHandler<PassTurnArgs, Unit>
{
    public Task<Unit> Handle(PassTurnArgs request, CancellationToken cancellationToken)
    {
        // There's no need to do anything here, since the game loop will advance the turn after each action.
        return Unit.Task;
    }
}