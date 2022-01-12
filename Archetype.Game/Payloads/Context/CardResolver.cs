using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface IContextResolver
    {
        void Resolve(IContext context);
    }

    internal class ContextResolver : IContextResolver
    {
        private readonly IGameState _gameState;
        private readonly IHistoryWriter _historyWriter;

        public ContextResolver(IGameState gameState, IHistoryWriter historyWriter)
        {
            _gameState = gameState;
            _historyWriter = historyWriter;
        }
    
        public void Resolve(IContext context)
        {
            var results = new ResultsReaderWriter();
            
            foreach (var effect in context.EffectProvider.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }
            
            _historyWriter.Append(context, results);
        }
        
        
    }
}