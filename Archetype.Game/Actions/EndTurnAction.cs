using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Core;
using MediatR;

namespace Archetype.Game
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