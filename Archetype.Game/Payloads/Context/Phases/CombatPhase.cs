using Aqua.EnumerableExtensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal class CombatPhase : PhaseResolver, ICombatPhaseResolver
    {
        private readonly IGameState _gameState;

        public CombatPhase(IGameState gameState, IHistoryWriter historyWriter) : base(historyWriter)
        {
            _gameState = gameState;
        }

        protected override IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector)
        {
            foreach (var contestedNode in _gameState.Map.ContestedNodes())
            {
                contestedNode.ResolveCombat()
                    .ForEach(resultsReaderCollector.AddResult);
                contestedNode.BuryTheDead()
                    .ForEach(resultsReaderCollector.AddResult);
            }

            return resultsReaderCollector;
        }
    }
}