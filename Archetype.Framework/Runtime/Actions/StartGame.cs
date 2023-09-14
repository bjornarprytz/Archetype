using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record StartGameArgs(IEnumerable<string> DeckOfCards) : IRequest<Unit>;

// TODO: Make handler