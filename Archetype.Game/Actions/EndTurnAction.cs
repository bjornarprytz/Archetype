using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Context.Phase;
using MediatR;

namespace Archetype.Game.Actions
{
    public class EndTurnAction : IRequest { }

    public class EndTurnActionHandler : IRequestHandler<EndTurnAction>
    {
        private readonly IMovePhaseResolver _movePhase;

        public EndTurnActionHandler(IMovePhaseResolver movePhase)
        {
            _movePhase = movePhase;
        }
        
        public Task<Unit> Handle(EndTurnAction request, CancellationToken cancellationToken)
        {
            // This action signals that the player main phase is done
            
            // 1. Move
            
            // All enemies
            // Move towards player HQ

            _movePhase.Resolve();

            // combatResolver
            // check game over?
            // trigger structures (friendly, then enemy?)

            return Unit.Task;
        }
    }
}