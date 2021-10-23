using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aqua.TypeExtensions;
using Archetype.Core;
using Archetype.Game.Extensions;
using MediatR;

namespace Archetype.Game
{
    public class PlayCardAction : IRequest<int>
    {
        public long CardId { get; }
        public IEnumerable<long> TargetsIds { get; }

        public PlayCardAction(long cardId, IEnumerable<long> targetsIds)
        {
            CardId = cardId;
            TargetsIds = targetsIds;
        }
        
        public PlayCardAction(long cardId, params long[] targetsIds)
        {
            CardId = cardId;
            TargetsIds = targetsIds;
        }
        
        public PlayCardAction(long cardId)
        {
            CardId = cardId;
            TargetsIds = Enumerable.Empty<long>();
        }
    }
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction, int>
    {
        private readonly IGameState _gameState;

        public PlayCardActionHandler(IGameState gameState)
        {
            _gameState = gameState;
        }
        
        public async Task<int> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            if (!_gameState.IsPayerTurn)
                return 0; // Not the player's turn
            
            if (_gameState.GetGamePiece(request.CardId) is not ICard card)
                return 1; // GamePiece is not a card
            
            if (card.CurrentZone != _gameState.Player.Hand)
                return 2; // Card is not in hand
            
            if (card.Data.Cost > _gameState.Player.Resources)
                return 3; // Can't pay cost
            
            var targets = request.TargetsIds.Select(_gameState.GetGamePiece).ToList();

            if (targets.Any(t => t == null))
                return 4; // Some targets are null

            if (targets.Count != card.Data.Effects.Count)
                return 5; // Mismatching number of targets to effects
            
            foreach (var (effect, target) in card.Data.Effects.Select(effect => effect)
                                                                .Zip(targets.Select(target => target)))
            {
                if (!target.GetType().Implements(effect.TargetType))
                    return 6; // Some targets are illegal

                if (!effect.CallTargetValidationMethod(target, _gameState))
                    return 7; // Some targets are illegal
            }

            _gameState.Player.Resources -= card.Data.Cost;
            
            foreach (var (effect, target) in card.Data.Effects.Select(effect => effect)
                .Zip(targets.Select(target => target)))
            {
                effect.CallResolveMethod(target, _gameState);
            }


            return 8; // Success
        }
    }
}