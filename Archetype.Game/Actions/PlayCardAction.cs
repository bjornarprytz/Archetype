using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Exceptions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using MediatR;

namespace Archetype.Game.Actions
{
    public class PlayCardAction : IRequest
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
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
    {
        private readonly IGameState _gameState;
        private readonly ICardResolver _cardResolver;

        public PlayCardActionHandler(IGameState gameState, ICardResolver cardResolver)
        {
            _gameState = gameState;
            _cardResolver = cardResolver;
        }
        
        public Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            if (_gameState.GetGamePiece(request.CardGuid) is not ICard card)
                throw new PlayCardException("GamePiece is not a card");

            if (card.CurrentZone != _gameState.Player.Hand)
                throw new PlayCardException("Card is not in hand");

            if (card.Cost > _gameState.Player.Resources)
                throw new PlayCardException("Can't pay cost");

            if (card.Targets.Count() != request.TargetsGuids.Count())
                throw new PlayCardException("Invalid number of targets");
            
            var targets = request.TargetsGuids.Select(guid => _gameState.GetGamePiece(guid)).ToList();

            if (targets.Any(t => t == null))
                throw new PlayCardException("Some targets are null");

            
            
            _cardResolver.Resolve(card, targets); // TODO: Make this awaitable?


            return Unit.Task;
        }
    }
}