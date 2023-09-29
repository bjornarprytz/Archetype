using Archetype.Framework.Definitions;

namespace Archetype.Framework.Runtime.Implementation;

public class Definitions : IDefinitions, IDefinitionBuilder
{
    private readonly Dictionary<string, IKeywordDefinition> _keywords = new();

    public IKeywordDefinition? GetDefinition(string keyword)
    {
        return _keywords.TryGetValue(keyword, out var definition) ? definition : null;
    }

    public void AddKeyword(IKeywordDefinition keywordDefinition)
    {
        if (_keywords.ContainsKey(keywordDefinition.Name))
            throw new InvalidOperationException($"Keyword ({keywordDefinition.Name}) already exists");
        
        _keywords.Add(keywordDefinition.Name, keywordDefinition);
    }
}