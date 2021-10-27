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
    public class PlayCardAction : IRequest<string>
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
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction, string>
    {
        private readonly IGameState _gameState;

        public PlayCardActionHandler(IGameState gameState)
        {
            _gameState = gameState;
        }
        
        public async Task<string> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            if (!_gameState.IsPayerTurn)
                return "Not player's turn";

            if (_gameState.GetGamePiece(request.CardId) is not ICard card)
                return "GamePiece is not a card";

            if (card.CurrentZone != _gameState.Player.Hand)
                return "Card is not in hand";

            if (card.Data.Cost > _gameState.Player.Resources)
                return "Can't pay cost";
            
            var targets = request.TargetsIds.Select(_gameState.GetGamePiece).ToList();

            if (targets.Any(t => t == null))
                return "Some targets are null";

            if (targets.Count != card.Data.TargetData.Count)
                return $"Mismatching number of targets {targets.Count} != {card.Data.TargetData.Count}";
            
            foreach (var (targetData, chosenTarget) in card.Data.TargetData.Zip(targets))
            {
                if (!chosenTarget.GetType().Implements(targetData.TargetType))
                    return "Some targets are of the wrong type";

                if (!targetData.CallTargetValidationMethod(chosenTarget, _gameState))
                    return "Some targets are illegal";
            }

            _gameState.Player.Resources -= card.Data.Cost;
            
            foreach (var effect in card.Data.Effects)
            {
                if (effect.TargetIndex >= targets.Count)
                    return $"Target index ({effect.TargetIndex}) out of range {targets.Count}";
                
                var target = targets[effect.TargetIndex];
                
                effect.CallResolveMethod(target, _gameState);
            }


            return "Resolves."; // Success
        }
    }
}