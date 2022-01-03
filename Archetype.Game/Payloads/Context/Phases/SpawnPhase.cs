using System.Linq;
using Archetype.Game.Payloads.Context.Phases.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal class SpawnPhase : PhaseResolver, ISpawnPhaseResolver
    {
        private readonly IMap _map;
        private readonly IProtoPool _protoPool;


        private int difficulty = 5; // TODO: inject this from somewhere else

        public SpawnPhase(IMap map, IProtoPool protoPool, IHistoryWriter historyWriter) : base(historyWriter)
        {
            _map = map;
            _protoPool = protoPool;
        }

        protected override IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector)
        {
            var spawnedLevel = 0;

            while (spawnedLevel < difficulty)
            {
                var creature = _protoPool.Creatures.First();

                spawnedLevel += creature.MetaData.Level + 1; // At least 1

                resultsReaderCollector.AddResult(_map.Nodes.Last().CreateCreature(creature.Name, default));
            }

            return resultsReaderCollector;
        }
    }
}