using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using Archetype.Rules.Extensions;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Encounter;

public class EndTurn
{
    public record Command() : IRequest<IActionResult>;
    
    public class Handler : IRequestHandler<Command, IActionResult>
    {
        private readonly IGameState _gameState;
        private readonly Random _random;

        public Handler(IGameState gameState, Random random)
        {
            _gameState = gameState;
            _random = random;
        }
        
        public Task<IActionResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var results = new List<IResult>
            {
                _gameState.ResolveUpkeep(_random),
                _gameState.ResolveCombat(_random),
                _gameState.ResolveMovement(_random)
            };

            return Task.FromResult<IActionResult>(new ActionResult(results));
        }
        
        public class Validator : AbstractValidator<Command>
        {
            public Validator(IGameState gameState)
            {
                RuleFor(x => x)
                    .Must(x => gameState.Prompter.CurrentPrompt == null)
                    .WithMessage("Cannot end turn while a prompt is active");

                RuleFor(x => x)
                    .Must(x => gameState.CurrentLocation != null)
                    .WithMessage("No location is currently active.");
            }
        }
    }
}