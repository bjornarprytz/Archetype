using System.Collections.Generic;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Trigger
{
    public interface ITriggerResolver<in TSource>
        where TSource : IGameAtom, ITriggerSource<TSource>
    {
        void Resolve(TSource source);
    }
    
    public interface ITriggerContext<out TSource> : IContext
        where TSource : IGameAtom
    {
        TSource Source { get; }
    }
    
    public interface ITriggerSource<in TSource>
        where TSource : IGameAtom
    {
        IEnumerable<IEffect<ITriggerContext<TSource>>> Effects { get; }
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
            var results = new ResultsReaderWriter();

            var context = new TriggerContext(_gameState, results, source, source.TopOwner());

            foreach (var effect in source.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }
            
            _historyWriter.Append(results);
        }

        private record TriggerContext(
                IGameState GameState, 
                IResultsReader PartialResults, 
                TSource Source,
                IGameAtom Owner)
            : ITriggerContext<TSource>;
    }
}