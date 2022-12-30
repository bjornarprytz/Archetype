namespace Archetype.Core.Effects;

public interface IResult
{
    IEnumerable<IEffectResult> Results { get; }

    static IResult Join(IEnumerable<IResult> results)
    {
        return new Result(results);
    }
    
    static IResult Empty()
    {
        return new Result();
    }
}

public interface IEffectResult
{
    string Keyword { get; }
    IReadOnlyDictionary<string, string> Data { get; }
}

file class Result : IResult
{
    private readonly List<IEffectResult> _results = new ();
    
    public Result() { }
    public Result(IEnumerable<IResult> results)
    {
        _results = results.SelectMany(r => r.Results).ToList();
    }

    public Result(IEnumerable<IEffectResult> effectResults)
    {
        _results = effectResults.ToList();
    }

    public IEnumerable<IEffectResult> Results => _results;
}