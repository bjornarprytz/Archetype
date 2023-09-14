using MediatR;

namespace Archetype.Framework.Runtime.Actions;

public record PassTurnArgs() : IRequest<Unit>;

// TODO: Make handler