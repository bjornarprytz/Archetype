using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.Design;

public interface IRules
{
    IEnumerable<IKeywordDefinition> Keywords { get; }
    IKeywordDefinition? GetDefinition(string keyword);
    T? GetDefinition<T>() where T : IKeywordDefinition;
}

public class Rules : IRules
{
    private readonly Dictionary<string, IKeywordDefinition> _keywords;
    private readonly Dictionary<Type, IKeywordDefinition> _keywordsByType;
    
    public Rules(Dictionary<string, IKeywordDefinition> keywords, IReadOnlyList<IPhase> turnSequence)
    {
        _keywords = keywords;
        _keywordsByType = keywords.ToDictionary(x => x.Value.GetType(), x => x.Value);
        
        TurnSequence = turnSequence;
    }

    public IReadOnlyList<IPhase> TurnSequence { get; }

    public IEnumerable<IKeywordDefinition> Keywords => _keywords.Values;

    public IKeywordDefinition? GetDefinition(string keyword)
    {
        return _keywords.TryGetValue(keyword, out var definition) ? definition : null;
    }

    public T? GetDefinition<T>() where T : IKeywordDefinition
    {
        if (_keywordsByType.TryGetValue(typeof(T), out var definition))
            return default;
        
        if (definition is T typedDefinition)
            return typedDefinition;
        
        throw new InvalidOperationException($"Keyword ({typeof(T).Name}) is not a {nameof(IKeywordDefinition)}");
    }

    
}