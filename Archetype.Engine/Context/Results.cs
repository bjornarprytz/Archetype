using Archetype.Core.Infrastructure;
using Archetype.Core.Play;

namespace Archetype.Engine.Context;

internal class ResultsReaderWriter : IResultsReaderWriter
{
    private readonly List<IEffectResult> _results = new();

    public IEnumerable<IEffectResult> GetResults()
    {
        return _results.SelectMany(Flatten);
        
        static IEnumerable<IEffectResult> Flatten(IEffectResult result)
        {
            return result.SideEffects.SelectMany(Flatten).Where(e => !e.IsNull);
        }
    }
    
    

    public void AddResult(IEffectResult effectEffectResult)
    {
        _results.Add(effectEffectResult);
    }
    
}