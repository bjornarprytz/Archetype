using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record AnswerPromptArgs(Guid PromptId, IReadOnlyList<Guid> Answer) : IRequest<Unit>;

public class AnswerPromptHandler(IActionQueue actionQueue, IGameState gameState) : IRequestHandler<AnswerPromptArgs, Unit>
{
    public Task<Unit> Handle(AnswerPromptArgs request, CancellationToken cancellationToken)
    {
        if (actionQueue.CurrentFrame == null)
            throw new InvalidOperationException("No prompt to answer");
        
        var selection = request.Answer.Select(gameState.GetAtom).ToList();
        
        actionQueue.CurrentFrame.Context.PromptResponses.Add(request.PromptId, selection);

        return Unit.Task;
    }
}