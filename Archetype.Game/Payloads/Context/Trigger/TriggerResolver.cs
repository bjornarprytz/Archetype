using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Trigger
{
    public interface ITriggerResolver<in TSource>
        where TSource : IGameAtom, ITriggerSource<TSource>
    {
        void Resolve(TSource source);
    }
    
    public interface ITriggerContext<out TSource> : IResolutionContext
        where TSource : IGameAtom
    {
        TSource Source { get; }
    }
    
    public class TriggerResolver<TSource> : ITriggerResolver<TSource> 
        where TSource : IGameAtom, ITriggerSource<TSource>
    {
        private readonly IGameState _gameState;
        private readonly IHistoryWriter _historyWriter;

        public TriggerResolver(IGameState gameState, IHistoryWriter historyWriter)
        {
            _gameState = gameState;
            _historyWriter = historyWriter;
        }
        
        public void Resolve(TSource source)
        {
            var results = new ResolutionCollector();

            using var context = new TriggerContext(_gameState, results, source);

            foreach (var effect in source.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }
            
            _historyWriter.Append(results);
        }
        
        private class TriggerContext : ITriggerContext<TSource>
        {
            public TriggerContext(IGameState gameState, IResolution partialResults, TSource source)
            {
                GameState = gameState;
                PartialResults = partialResults;
                Source = source;
            }

            public IGameState GameState { get; }
            public IResolution PartialResults { get; }
            public TSource Source { get; }
        
            public void Dispose() { }
        }
    }
}