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
            // This action signals that the player main phase is done
            
            // 1. Move
            
            // All enemies
            // Move towards player HQ
            
            // 2. Resolve Combat
            
            // 3. Player Draws a card and starts turn 

            return "Enemy turn executed!";
        }
    }
}