using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record AnswerPromptArgs(IReadOnlyList<Guid> Answer) : IRequest<Unit>;

public class AnswerPromptHandler : IRequestHandler<AnswerPromptArgs, Unit>
{
    private readonly IActionQueue _actionQueue;
    private readonly IGameState _gameState;

    public AnswerPromptHandler(IActionQueue actionQueue, IGameState gameState)
    {
        _actionQueue = actionQueue;
        _gameState = gameState;
    }
    
    public Task<Unit> Handle(AnswerPromptArgs request, CancellationToken cancellationToken)
    {
        if (_actionQueue.CurrentFrame == null)
            throw new InvalidOperationException("No prompt to answer");
        
        var selection = request.Answer.Select(_gameState.GetAtom).ToList();
        
        _actionQueue.CurrentFrame.Context.PromptResponses.Add(selection);

        return Unit.Task;
    }
}