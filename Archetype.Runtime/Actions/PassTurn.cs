using MediatR;

namespace Archetype.Runtime.Actions;

public record PassTurnArgs() : IRequest<ActionResult>;
