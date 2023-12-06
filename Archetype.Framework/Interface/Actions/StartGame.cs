using MediatR;

namespace Archetype.Framework.Interface.Actions;

public record StartGameArgs(IEnumerable<string> DeckOfCards) : IRequest<Unit>;

// TODO: Make handler