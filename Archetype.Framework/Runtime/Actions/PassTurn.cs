using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record PassTurnArgs() : IRequest<Unit>;

public class PassTurnHandler : IRequestHandler<PassTurnArgs, Unit>
{
    private readonly IActionQueue _actionQueue;

    public PassTurnHandler(IActionQueue actionQueue)
    {
        _actionQueue = actionQueue;
    }
    
    public Task<Unit> Handle(PassTurnArgs request, CancellationToken cancellationToken)
    {
        // There's no need to do anything here, since the game loop will advance the turn after each action.
        return Unit.Task;
    }
}