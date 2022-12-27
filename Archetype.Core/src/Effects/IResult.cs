namespace Archetype.Core.Effects;

public interface IResult
{
    IEnumerable<IEffectResult> Results { get; }

    void Add(IEffectResult result);
    void Concat(IResult result);
    
    static IResult Join(IEnumerable<IResult> results)
    {
        return new Result(results);
    }
    
    static IResult Create()
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
    
    public Result()
    {
        _results = new List<IEffectResult>();
    }
    public Result(IEnumerable<IResult> results)
    {
        _results = results.SelectMany(r => r.Results).ToList();
    }

    public Result(IEnumerable<IEffectResult> effectResults)
    {
        _results = effectResults.ToList();
    }

    public IEnumerable<IEffectResult> Results => _results;
    public void Add(IEffectResult result)
    {
        _results.Add(result);
    }

    public void Concat(IResult result)
    {
        _results.AddRange(result.Results);
    }
}