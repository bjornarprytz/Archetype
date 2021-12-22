using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    internal interface IPhaseResolver { void Resolve(); }
    internal interface IMovePhaseResolver : IPhaseResolver { }
    internal interface ICombatPhaseResolver : IPhaseResolver {}
    internal interface IUpkeepPhaseResolver : IPhaseResolver {}
    internal interface ISpawnPhaseResolver : IPhaseResolver {}

    internal abstract class PhaseResolver : IPhaseResolver
    {
        private readonly IHistoryWriter _historyWriter;

        protected PhaseResolver(IHistoryWriter historyWriter)
        {
            _historyWriter = historyWriter;
        }
        
        public void Resolve()
        {
            _historyWriter.Append(ResolvePhase(new ResultsReaderWriter()));
        }

        protected abstract IResultsReader ResolvePhase(IResultsReaderWriter resultsReaderCollector);
    }
}