using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record AnswerPromptArgs(IReadOnlyList<Guid> Answer) : IRequest<Unit>;