using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Context.Phases;
using Archetype.Game.Payloads.Context.Phases.Base;
using MediatR;

namespace Archetype.Server.Actions
{
    public class EndTurnAction : IRequest { }

    internal class EndTurnActionHandler : IRequestHandler<EndTurnAction>
    {
        private readonly IMovePhaseResolver _movePhase;
        private readonly ICombatPhaseResolver _combatPhase;
        private readonly IUpkeepPhaseResolver _upkeepPhase;

        public EndTurnActionHandler(
            IMovePhaseResolver movePhase,
            ICombatPhaseResolver combatPhase,
            IUpkeepPhaseResolver upkeepPhase)
        {
            _movePhase = movePhase;
            _combatPhase = combatPhase;
            _upkeepPhase = upkeepPhase;
        }
        
        public Task<Unit> Handle(EndTurnAction request, CancellationToken cancellationToken)
        {
            _movePhase.Resolve();
            _combatPhase.Resolve();
            // TODO: check game over?
            _upkeepPhase.Resolve();
            // TODO: check game over?
            return Unit.Task;
        }
    }
}