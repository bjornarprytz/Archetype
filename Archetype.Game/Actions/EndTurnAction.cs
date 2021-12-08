using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Extensions;
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

            // TODO: Replace this with phase structure, where each phase executes their 
            
            foreach (var enemy in _gameState.Map.EachEnemyCreature())
            {
                // TODO: Move enemy towards player HQ
            }

            foreach (var node in _gameState.Map.Nodes)
            {
                // TODO: Resolve combat
            }
            
            // TODO: Check game over 

            foreach (var friendlyStructure in _gameState.Map.EachFriendlyStructure())
            {
                // TODO: Trigger effect    
            }

            foreach (var enemyStructure in _gameState.Map.EachEnemyStructure())
            {
                // TODO: Trigger effect
            }

            return "Enemy turn executed!";
        }
    }
}