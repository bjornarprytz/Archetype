using Archetype.Core.Infrastructure;
using Archetype.Core.Play;
using Archetype.Engine.Context;

namespace Archetype.Engine;

public interface IContextResolver
{
    void Resolve(IContext context);
}

internal class ContextResolver : IContextResolver
{
    private readonly IHistoryWriter _historyWriter;

    public ContextResolver(IHistoryWriter historyWriter)
    {
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