using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record AnswerPromptArgs(Guid PromptId, IReadOnlyList<Guid> Answer) : IRequest<Unit>;

public class AnswerPromptHandler(IActionQueue actionQueue, IGameState gameState) : IRequestHandler<AnswerPromptArgs, Unit>
{
    public Task<Unit> Handle(AnswerPromptArgs request, CancellationToken cancellationToken)
    {
        var selection = request.Answer.Select(gameState.GetAtom).ToList();

        var result = actionQueue.ResolvePrompt(new PromptContext(request.PromptId, selection));
        
        if (result is FailureResult failure)
            throw new InvalidOperationException(failure.Message);

        return Unit.Task;
    }
}