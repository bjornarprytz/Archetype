using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Infrastructure;
using MediatR;

namespace Archetype.Game.Actions
{
    public class EndTurnAction : IRequest<string> { }

    public class EndTurnActionHandler : IRequestHandler<EndTurnAction, string>
    {
        private readonly IGameState _gameState;

        public EndTurnActionHandler(IGameState gameState)
        {
            _gameState = gameState;
        }
        
        public async Task<string> Handle(EndTurnAction request, CancellationToken cancellationToken)
        {
            // TODO: Execute enemy turn

            return "Enemy turn executed!";
        }
    }
}