using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aqua.TypeExtensions;
using Archetype.Core;
using Archetype.Game.Extensions;
using MediatR;

namespace Archetype.Game
{
    public class PlayCardAction : IRequest
    {
        public long CardId { get; }
        public IOrderedEnumerable<long> TargetsIds { get; }

        public PlayCardAction(long cardId, IOrderedEnumerable<long> targetsIds)
        {
            CardId = cardId;
            TargetsIds = targetsIds;
        }
        
        public PlayCardAction(long cardId)
        {
            CardId = cardId;
            TargetsIds = (IOrderedEnumerable<long>)Enumerable.Empty<long>();
        }
    }
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
    {
        private readonly IGameState _gameState;

        public PlayCardActionHandler(IGameState gameState)
        {
            _gameState = gameState;
        }
        
        public async Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            if (_gameState.GetGamePiece(request.CardId) is not ICard card)
                return Unit.Value; // GamePiece is not a card
            
            if (card.CurrentZone != _gameState.Player.Hand.Cards)
                return Unit.Value; // Card is not in hand
            
            if (card.Data.Cost > _gameState.Player.Resources)
                return Unit.Value; // Can't pay cost
            
            var targets = request.TargetsIds.Select(_gameState.GetGamePiece).ToList();

            if (targets.Any(t => t == null))
                return Unit.Value; // Some targets are null
            
            foreach (var (effect, target) in card.Data.Effects.Select(effect => effect)
                                                                .Zip(targets.Select(target => target)))
            {
                if (!target.GetType().Implements(effect.TargetType))
                    return Unit.Value; // Some targets are illegal

                if (!effect.CallTargetValidationMethod(target, _gameState))
                    return Unit.Value; // Some targets are illegal
            }
            
            foreach (var (effect, target) in card.Data.Effects.Select(effect => effect)
                .Zip(targets.Select(target => target)))
            {
                effect.CallResolveMethod(target, _gameState);
            }


            return Unit.Value; // Success
        }
    }
}