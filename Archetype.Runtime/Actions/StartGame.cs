using MediatR;

namespace Archetype.Runtime.Actions;

public record StartGameArgs(IEnumerable<string> DeckOfCards) : IRequest<Unit>;