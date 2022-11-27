using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using Archetype.Core.Proto.PlayingCard;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Encounter;

public class PlayCard
{
    public record Command(Guid CardId, List<Guid> PaymentCardIds, List<Guid> TargetGuids) : IRequest<IEnumerable<Guid>>;

    private record PlayContext(IGameState GameState, ICard Source, ITargetProvider TargetProvider) : IContext<ICard>;

    public class Handler : IRequestHandler<Command, IEnumerable<Guid>>
    {
        private readonly IGameState _gameState;
        private readonly IAtomFinder _atomFinder;
        private readonly IProtoFinder _protoFinder;

        public Handler(IGameState gameState, IAtomFinder atomFinder, IProtoFinder protoFinder)
        {
            _gameState = gameState;
            _atomFinder = atomFinder;
            _protoFinder = protoFinder;
        }
        
        public Task<IEnumerable<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var cardToPlay = _atomFinder.FindAtom<ICard>(request.CardId);
            if (cardToPlay.CurrentZone != _gameState.Player.Hand)
                throw new InvalidOperationException("Cannot play a card that is not in your hand.");
            
            var protoCard = _protoFinder.FindProto<IProtoPlayingCard>(cardToPlay.ProtoId);
            
            
            var targets = request.TargetGuids.Select(_atomFinder.FindAtom);

            
            var paymentCards = request.PaymentCardIds.Select(_atomFinder.FindAtom<ICard>).ToList();
            if (paymentCards.Any(p => p.CurrentZone != _gameState.Player.Hand))
            {
                throw new Exception("Cannot pay with a card that is not in your hand.");
            }
            
            // TODO: Make cost available on the card so we don't have to look it up through the proto.
            var paymentProtoCards = paymentCards.Select(card => _protoFinder.FindProto<IProtoPlayingCard>(card.ProtoId));
            if (paymentProtoCards.Sum(p => p.Resources) < protoCard.Cost)
            {
                throw new Exception("Not enough resources to play this card.");
            }
            
            
            foreach (var card in paymentCards)
            {
                card.MoveTo(_gameState.Player.DiscardPile);
            }
            
            var result = protoCard.Resolve(new PlayContext(_gameState, cardToPlay,
                new TargetProvider(targets, protoCard.TargetDescriptors)));
            
            return Task.FromResult(result.AffectedAtoms.Concat(request.PaymentCardIds));
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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

