using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases.Base
{
    public interface IPhaseResolver { void Resolve(); }

    public interface IMovePhaseResolver : IPhaseResolver { }

    public interface ICombatPhaseResolver : IPhaseResolver {}

    public interface IUpkeepPhaseResolver : IPhaseResolver {}

    public interface ISpawnPhaseResolver : IPhaseResolver {}

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