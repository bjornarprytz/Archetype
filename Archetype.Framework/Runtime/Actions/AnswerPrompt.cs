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
        // TODO: Figure out how to validate answers to prompts

        if (_actionQueue.CurrentContext == null)
            throw new InvalidOperationException("No prompt to answer");
        
        var selection = request.Answer.Select(_gameState.GetAtom).ToList();
        
        _actionQueue.CurrentContext.PromptResponses.Add(selection);

        return Unit.Task;
    }
}