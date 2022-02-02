using Archetype.Core.Infrastructure;
using Archetype.Core.Play;

namespace Archetype.Engine.Context;

internal class ResultsReaderWriter : IResultsReaderWriter
{
    private readonly List<IEffectResult> _results = new();

    public IEnumerable<IEffectResult> Results => _results;

    public void AddResult(IEffectResult effectEffectResult)
    {
        _results.Add(effectEffectResult);
    }
}