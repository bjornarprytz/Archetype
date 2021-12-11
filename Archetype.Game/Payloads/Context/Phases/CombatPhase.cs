using Aqua.EnumerableExtensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    public class CombatPhase : PhaseResolver, ICombatPhaseResolver
    {
        private readonly IGameState _gameState;

        public CombatPhase(IGameState gameState, IHistoryWriter historyWriter) : base(historyWriter)
        {
            _gameState = gameState;
        }

        protected override IResolution ResolvePhase(IResolutionCollector resultsCollector)
        {
            foreach (var contestedNode in _gameState.Map.ContestedNodes())
            {
                contestedNode.ResolveCombat()
                    .ForEach(resultsCollector.AddResult);
                contestedNode.BuryTheDead()
                    .ForEach(resultsCollector.AddResult);
            }

            return resultsCollector;
        }
    }
}