using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aqua.TypeExtensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Pieces;
using MediatR;

namespace Archetype.Game.Actions
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

            if (card.Cost > _gameState.Player.Resources)
                return "Can't pay cost";
            
            var targets = request.TargetsIds.Select(_gameState.GetGamePiece).ToList();

            if (targets.Any(t => t == null))
                return "Some targets are null";

            var cardResolutionContext = new CardResolutionContext(_gameState, _gameState.Player, targets);

            if (card.ValidateTargets(cardResolutionContext))
                return "Invalid targets";

            _gameState.Player.Resources -= card.Cost;

            card.Resolve(cardResolutionContext);
            
            return "Resolves."; // Success
        }
    }
}