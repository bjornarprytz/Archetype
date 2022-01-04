using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Design;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Context;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Server.Actions
{
    public class StartGameAction : IRequest<ITurnContext>
    {

        public StartGameAction()
        {
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction, ITurnContext>
    {
        private readonly IDesign _design;
        private readonly IPlayer _player;
        private readonly IPlayerData _playerData;
        private readonly IFactory<ITurnContext> _turnFactory;
        private readonly IInstanceFactory _instanceFactory;

        public StartGameActionHandler(
            IDesign design, 
            IPlayer player,
            IPlayerData playerData,
            IFactory<ITurnContext> turnFactory,
            IInstanceFactory instanceFactory)
        {
            _design = design;
            _player = player;
            _playerData = playerData;
            _turnFactory = turnFactory;
            _instanceFactory = instanceFactory;
        }
        
        public async Task<ITurnContext> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            _design.Create();
            
            foreach (var cardProtoData in _playerData.DeckList)
            {
                var card = _instanceFactory.CreateCard(cardProtoData, _player);

                _player.Deck.PutCardOnTop(card);
            }

            _player.Deck.Shuffle();

            _player.Draw(_player.MaxHandSize);

            return _turnFactory.Create();
        }
    }
}