using Archetype.Core.Infrastructure;
using Archetype.Rules.Extensions;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Encounter;

public class EndTurn
{
    public record Command() : IRequest<Unit>;
    
    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IGameState _gameState;
        private readonly Random _random;

        public Handler(IGameState gameState, Random random)
        {
            _gameState = gameState;
            _random = random;
        }
        
        public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            
            _gameState.ResolveUpkeep(_random);
            _gameState.ResolveCombat(_random);
            _gameState.ResolveMovement(_random);

            return Unit.Task;
        }
        
        public class Validator : AbstractValidator<Command>
        {
            public Validator(IGameState gameState)
            {
                RuleFor(x => x)
                    .Must(x => gameState.CurrentLocation != null)
                    .WithMessage("No location is currently active.");
            }
        }
    }
}