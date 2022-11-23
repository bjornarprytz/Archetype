using MediatR;

namespace Archetype.Rules.Encounter;

public class EndTurn
{
    public record Command() : IRequest<Unit>;
    
    public class Handler : IRequestHandler<Command, Unit>
    {
        public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            // TODO: Enemy Units take their turn

            return Unit.Task;
        }
    }
}