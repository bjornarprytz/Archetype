using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal class UpkeepPhase : PhaseResolver, IUpkeepPhaseResolver
    {
        private readonly IGameState _gameState;
        private readonly ITriggerResolver<IStructure> _triggerResolver;

        public UpkeepPhase(IGameState gameState, IHistoryWriter historyWriter, ITriggerResolver<IStructure> triggerResolver) : base(historyWriter)
        {
            _gameState = gameState;
            _triggerResolver = triggerResolver;
        }

        protected override IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector)
        {
            foreach (var friendlyStructure in _gameState.Map.EachFriendlyStructure())
            {
                _triggerResolver.Resolve(friendlyStructure);
            }

            foreach (var enemyStructure in _gameState.Map.EachEnemyStructure())
            {
                _triggerResolver.Resolve(enemyStructure);
            }


            return resultsReaderCollector; 
        }
    }
}