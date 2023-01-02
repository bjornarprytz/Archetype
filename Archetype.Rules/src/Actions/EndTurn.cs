using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using Archetype.Rules.Extensions;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Actions;

public class EndTurn
{
    public record Command(List<Guid> CardsToKeep) : IRequest<IActionResult>;
    
    public class Handler : IRequestHandler<Command, IActionResult>
    {
        private readonly IGameState _gameState;
        private readonly IAtomFinder _atomFinder;
        private readonly Random _random;

        public Handler(IGameState gameState, IAtomFinder atomFinder, Random random)
        {
            _gameState = gameState;
            _atomFinder = atomFinder;
            _random = random;
        }
        
        public Task<IActionResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var cardsToKeep = request.CardsToKeep.Select(_atomFinder.FindAtom<ICard>).ToList();
            if (cardsToKeep.Any(p => p.CurrentZone != _gameState.Player.Hand))
            {
                throw new Exception("Cannot keep cards that are in your hand");
            }
            
            var cardsToDiscard = _gameState.Player.Hand.Contents.Except(cardsToKeep);
            
            var results = new List<IResult>
            {
                IResult.Join(cardsToDiscard.Select(card => card.MoveTo(_gameState.Player.DiscardPile))),
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
                
                RuleFor(x => x.CardsToKeep.Count)
                    .Must(cardCount => cardCount <= gameState.Player.Hand.Capacity)
                    .WithMessage("Cannot keep more cards than the hand capacity.");
            }
        }
    }
}