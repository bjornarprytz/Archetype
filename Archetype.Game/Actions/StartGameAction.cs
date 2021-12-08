using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Exceptions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Game.Actions
{
    public class StartGameAction : IRequest
    {
        public IEnumerable<Guid> DeckList { get; }
        
        public Guid HQStructureGuid { get; }
        public Guid HQPlacement { get; }

        public StartGameAction(IEnumerable<Guid> deckList, Guid hqStructureGuid, Guid hqPlacement)
        {
            DeckList = deckList;
            HQStructureGuid = hqStructureGuid;
            HQPlacement = hqPlacement;
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction>
    {
        private readonly IProtoPool _protoPool;
        private readonly IPlayer _player;
        private readonly IMap _map;

        public StartGameActionHandler(IProtoPool protoPool, IPlayer player, IMap map)
        {
            _protoPool = protoPool;
            _player = player;
            _map = map;
        }
        
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            var headQuarter = _protoPool.CreateStructure(request.HQStructureGuid, _player);

            if (headQuarter is null)
                throw new StartGameException($"HQ is of non-existent structure ({request.HQStructureGuid}");
                
            
            var hqPlacement = _map.Nodes.FirstOrDefault(n => n.Guid == request.HQPlacement);

            if (hqPlacement is null)
                throw new StartGameException($"HQ placed in non existent area ({request.HQPlacement}");

            foreach (var guid in request.DeckList)
            {
                var card = _protoPool.CreateCard(guid, _player);

                if (card is null)
                    throw new StartGameException($"Deck contains non-existent card ({guid})");
                
                _player.Deck.PutCardOnTop(card);
            }
            
            _player.Deck.Shuffle();

            if (_player.Deck.Contents.Count() < _player.MinDeckSize)
                throw new StartGameException("Deck is too small");
            
            _player.Draw(_player.MaxHandSize);
            
            // TODO: Set up map positions as well? HQ for instance.

            return Unit.Task;
        }
    }
}