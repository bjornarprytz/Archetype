using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Pieces;
using MediatR;

namespace Archetype.Game.Actions
{
    public class SaveDeckAction : IRequest
    {
        public SaveDeckAction(IEnumerable<Guid> cardGuids)
        {
            CardGuids = cardGuids;
        }

        public IEnumerable<Guid> CardGuids { get; }
    }
    
    public class SaveDeckActionHandler : IRequestHandler<SaveDeckAction>
    {
        private readonly IGameState _gameState;
        private readonly ICardPool _cards;

        public SaveDeckActionHandler(IGameState gameState, ICardPool cards)
        {
            _gameState = gameState;
            _cards = cards;
        }
        
        public Task<Unit> Handle(SaveDeckAction request, CancellationToken cancellationToken)
        {
            foreach (var cardGuid in request.CardGuids)
            {
                _gameState.Player.Deck.PutCardOnTop(_cards[cardGuid].CreateInstance());
            }

            return Unit.Task;
        }
    }
}