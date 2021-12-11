using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Phases
{
    public interface IPhaseResolver { void Resolve(); }
    public interface IMovePhaseResolver : IPhaseResolver { }
    public interface ICombatPhaseResolver : IPhaseResolver {}
    public interface IUpkeepPhaseResolver : IPhaseResolver {}

    public abstract class PhaseResolver : IPhaseResolver
    {
        private readonly IHistoryWriter _historyWriter;

        protected PhaseResolver(IHistoryWriter historyWriter)
        {
            _historyWriter = historyWriter;
        }
        
        public void Resolve()
        {
            _historyWriter.Append(ResolvePhase(new ResolutionCollector()));
        }

        protected abstract IResolution ResolvePhase(IResolutionCollector resultsCollector);
    }
}