using Aqua.EnumerableExtensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal class MovePhase : PhaseResolver, IMovePhaseResolver
    {
        private readonly IGameState _gameState;

        public MovePhase(IGameState gameState, IHistoryWriter historyWriter) : base(historyWriter)
        {
            _gameState = gameState;
        }
        protected override IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector)
        { 
            var shortestPaths = (_gameState.Player.HeadQuarters.CurrentZone as IMapNode).CalculateShortestPaths();
            
            foreach (var enemy in _gameState.Map.EachEnemyCreature())
            {
                enemy.MoveAlong(shortestPaths, _gameState.Player.HeadQuarters.CurrentZone as IMapNode)
                    .ForEach(resultsReaderCollector.AddResult);
            }

            return resultsReaderCollector;
        }
    }
}