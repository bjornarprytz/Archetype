using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.State;

namespace Archetype.Framework.DependencyInjection;

public interface IRulesBuilder
{
    void AddKeyword(IKeywordDefinition keywordDefinition);
    void AddPhase(IPhase phase);
    
    IRules Build();
}

public class RulesBuilder : IRulesBuilder
{
    private readonly Dictionary<string, IKeywordDefinition> _keywords = new();
    private readonly List<IPhase> _turnSequence = new();
    
    public void AddKeyword(IKeywordDefinition keywordDefinition)
    {
        if (_keywords.ContainsKey(keywordDefinition.Id))
            throw new InvalidOperationException($"Keyword ({keywordDefinition.Id}) already exists");
        
        _keywords.Add(keywordDefinition.Id, keywordDefinition);
    }

    public void AddPhase(IPhase phase)
    {
        _turnSequence.Add(phase);
    }

    public IRules Build()
    {
        // TODO: Validate rules
        
        return new Rules(_keywords, _turnSequence);
    }
}