using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Infrastructure;
using MediatR;

namespace Archetype.Game.Actions
{
    public class EndTurnAction : IRequest { }

    public class EndTurnActionHandler : IRequestHandler<EndTurnAction>
    {
        private readonly IGameState _gameState;

        public EndTurnActionHandler(IGameState gameState)
        {
            _gameState = gameState;
        }
        
        public async Task<Unit> Handle(EndTurnAction request, CancellationToken cancellationToken)
        {
            _gameState.IsPayerTurn = false;

            return Unit.Value;
        }
    }
}