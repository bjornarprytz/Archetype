using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class Rules : IRules, IRulesBuilder
{
    private readonly Dictionary<string, IKeywordDefinition> _keywords = new();
    private readonly Dictionary<Type, IKeywordDefinition> _keywordsByType = new();

    public IReadOnlyList<IPhase> Phases { get; private set; } = Array.Empty<IPhase>();

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

    public void AddKeyword(IKeywordDefinition keywordDefinition)
    {
        if (_keywords.ContainsKey(keywordDefinition.Name))
            throw new InvalidOperationException($"Keyword ({keywordDefinition.Name}) already exists");
        
        _keywords.Add(keywordDefinition.Name, keywordDefinition);
        _keywordsByType.Add(keywordDefinition.GetType(), keywordDefinition);
    }

    public void SetTurnSequence(IReadOnlyList<IPhase> phase)
    {
        Phases = phase;
    }
}