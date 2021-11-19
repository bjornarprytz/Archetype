using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Game.Actions
{
    public class StartGameAction : IRequest
    {
        public IEnumerable<Guid> DeckList { get; }

        public StartGameAction(IEnumerable<Guid> deckList)
        {
            DeckList = deckList;
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction>
    {
        private readonly ICardPool _cardPool;
        private readonly IPlayer _player;

        public StartGameActionHandler(ICardPool cardPool, IPlayer player)
        {
            _cardPool = cardPool;
            _player = player;
        }
        
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            foreach (var guid in request.DeckList)
            {
                var protoCard = _cardPool[guid];
                
                _player.Deck.PutCardOnTop(new Card(protoCard, _player));
            }

            return Unit.Task;
        }
    }
}