using System.Threading;
using System.Threading.Tasks;
using Archetype.Design;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Infrastructure;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Server.Actions
{
    public class StartGameAction : IRequest
    {

        public StartGameAction()
        {
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction>
    {
        private readonly IDesign _design;
        private readonly IPlayer _player;
        private readonly IPlayerData _playerData;
        private readonly IInstanceFactory _instanceFactory;

        public StartGameActionHandler(
            IDesign design, 
            IPlayer player,
            IPlayerData playerData,
            IInstanceFactory instanceFactory)
        {
            _design = design;
            _player = player;
            _playerData = playerData;
            _instanceFactory = instanceFactory;
        }
        
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            _design.Create();
            
            foreach (var cardProtoData in _playerData.DeckList)
            {
                var card = _instanceFactory.CreateCard(cardProtoData);
                card.SetOwner(_player);

                _player.Deck.PutCardOnTop(card);
            }

            _player.Deck.Shuffle();

            _player.Draw(_player.MaxHandSize);

            return Unit.Task;
        }
    }
}