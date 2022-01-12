using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Phases.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal class UpkeepPhase : PhaseResolver, IUpkeepPhaseResolver
    {
        private readonly IGameState _gameState;
        private readonly IContextResolver _contextResolver;
        private readonly IContextBinder _contextBinder;

        public UpkeepPhase(
            IGameState gameState, 
            IHistoryWriter historyWriter, 
            IContextResolver contextResolver,
            IContextBinder contextBinder
            ) : base(historyWriter)
        {
            _gameState = gameState;
            _contextResolver = contextResolver;
            _contextBinder = contextBinder;
        }

        protected override IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector)
        {
            /*
             * 
            
            foreach (var friendlyStructure in _gameState.Map.EachFriendlyStructure())
            {
            // TODO: Bind context
                _contextResolver.Resolve(friendlyStructure);
            }

            foreach (var enemyStructure in _gameState.Map.EachEnemyStructure())
            {
            // TODO: Bind context
                _contextResolver.Resolve(enemyStructure);
            }

             */

            return resultsReaderCollector; 
        }
    }
}