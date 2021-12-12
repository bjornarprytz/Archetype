using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Exceptions;
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
        private readonly IPlayer _player;
        private readonly IMap _map;
        private readonly IInstanceFactory _instanceFactory;
        private readonly IInstanceFinder _instanceFinder;

        public StartGameActionHandler(IPlayer player, IMap map, IInstanceFactory instanceFactory, IInstanceFinder instanceFinder)
        {
            _player = player;
            _map = map;
            _instanceFactory = instanceFactory;
            _instanceFinder = instanceFinder;
        }
        
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            var headQuarter = _instanceFactory.CreateStructure(request.HQStructureGuid, _player);
            var hqPlacement = _instanceFinder.FindAtom<IMapNode>(request.HQPlacement);

            headQuarter.MoveTo(hqPlacement);
            
            foreach (var guid in request.DeckList)
            {
                var card = _instanceFactory.CreateCard(guid, _player);
                
                _player.Deck.PutCardOnTop(card);
            }
            
            _player.Deck.Shuffle();

            if (_player.Deck.Contents.Count() < _player.MinDeckSize)
                throw new StartGameException("Deck is too small");
            
            _player.Draw(_player.MaxHandSize);

            return Unit.Task;
        }
    }
}