﻿using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Encounter;

public class PlayCard
{
    public record Command(Guid CardId, List<Guid> PaymentCardIds, List<Guid> TargetGuids) : IRequest<IActionResult>;

    private record PlayContext(IGameState GameState, ICard Source, ITargetProvider TargetProvider) : IContext<ICard>;

    public class Handler : IRequestHandler<Command, IActionResult>
    {
        private readonly IGameState _gameState;
        private readonly IAtomFinder _atomFinder;

        public Handler(IGameState gameState, IAtomFinder atomFinder)
        {
            _gameState = gameState;
            _atomFinder = atomFinder;
        }
        
        public Task<IActionResult> Handle(Command request, CancellationToken cancellationToken)
        {
            // TODO: Clean this up a bit
            
            var cardToPlay = _atomFinder.FindAtom<ICard>(request.CardId);
            if (cardToPlay.CurrentZone != _gameState.Player.Hand)
                throw new InvalidOperationException("Cannot play a card that is not in your hand.");
            
            var targets = request.TargetGuids.Select(_atomFinder.FindAtom);
            
            var paymentCards = request.PaymentCardIds.Select(_atomFinder.FindAtom<ICard>).ToList();
            if (paymentCards.Any(p => p.CurrentZone != _gameState.Player.Hand))
            {
                throw new Exception("Cannot pay with a card that is not in your hand.");
            }
            
            if (paymentCards.Sum(paymentCard => paymentCard.Proto.Stats.Resources) < cardToPlay.Proto.Stats.Cost)
            {
                throw new Exception("Not enough resources to play this card.");
            }

            var results = new List<IResult>(
                paymentCards.Select(card => card.MoveTo(_gameState.Player.DiscardPile)) // Discard payment
                )
            {
                cardToPlay.MoveTo(_gameState.ResolutionZone), // Move the card here so that it won't be affected by its own effects.
                cardToPlay.Proto.Resolve(new PlayContext(_gameState, cardToPlay,
                    new TargetProvider(targets, cardToPlay.Proto.TargetDescriptors))) // Resolve
            };
            
            return Task.FromResult<IActionResult>(new ActionResult(results));
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(IGameState gameState)
        {
            RuleFor(x => x)
                .Must(x => gameState.Prompter.CurrentPrompt == null)
                .WithMessage("Cannot play a card while a prompt is active.");
            
            RuleFor(a => a.CardId).Must((args, id) => args.PaymentCardIds.All(paymentId => paymentId != id))
                .WithMessage("Cannot pay with the card you are playing.");
            
            RuleFor(a => a.CardId).Must((args, id) => args.TargetGuids.All(targetGuid => targetGuid != id))
                .WithMessage("Cannot target the card you are playing.");
            
            RuleFor(a => a.PaymentCardIds).Must((paymentCardIds) => paymentCardIds.Count() == paymentCardIds.Distinct().Count())
                .WithMessage("Cannot pay with the same card twice.");

            RuleFor(a => a.TargetGuids).Must((targetGuids) => targetGuids.Count() == targetGuids.Distinct().Count())
                .WithMessage("Cannot target the same card twice.");
            
            RuleFor(a => a.PaymentCardIds).Must((args, paymentCardIds) => paymentCardIds.All(paymentGuid => args.TargetGuids.All(targetGuid => targetGuid != paymentGuid)))
                .WithMessage("Cannot pay with a card that is also targeted.");
        }
    }
}
