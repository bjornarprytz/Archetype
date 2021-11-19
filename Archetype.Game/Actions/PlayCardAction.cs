using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.PlayContext;
using MediatR;

namespace Archetype.Game.Actions
{
    public class PlayCardAction : IRequest<string>
    {
        public Guid CardGuid { get; }
        public IEnumerable<Guid> TargetsGuids { get; }
        
        public PlayCardAction(Guid cardGuid, IEnumerable<Guid> targetsGuids)
        {
            CardGuid = cardGuid;
            TargetsGuids = targetsGuids;
        }
        
        public PlayCardAction(Guid cardGuid, params Guid[] targetsGuids)
        {
            CardGuid = cardGuid;
            TargetsGuids = targetsGuids;
        }
        
        public PlayCardAction(Guid cardGuid)
        {
            CardGuid = cardGuid;
            TargetsGuids = Enumerable.Empty<Guid>();
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
            if (_gameState.GetGamePiece(request.CardGuid) is not ICard card)
                return "GamePiece is not a card";

            if (card.CurrentZone != _gameState.Player.Hand)
                return "Card is not in hand";

            if (card.Cost > _gameState.Player.Resources)
                return "Can't pay cost";

            if (card.Targets.Count() != request.TargetsGuids.Count())
                return "Invalid number of targets";
            
            var targets = request.TargetsGuids.Select(guid => _gameState.GetGamePiece(guid)).ToList();

            if (targets.Any(t => t == null))
                return "Some targets are null";

            var cardResolutionContext = new CardResolutionContext(_gameState, _gameState.Player, targets);

            if (!card.ValidateTargets(cardResolutionContext))
                return "Invalid targets";

            _gameState.Player.Resources -= card.Cost;

            card.Resolve(cardResolutionContext);
            
            // TODO: Formalize a verb for moving a card from hand to discard pile (must be different from Discard, to avoid those triggers)
            _gameState.Player.Hand.Remove(card);
            _gameState.Player.DiscardPile.Bury(card);

            return "Resolves."; // Success
        }
    }
}