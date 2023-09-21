using Archetype.Framework.Definitions;

namespace Archetype.Framework.Runtime.Implementation;

public class Definitions : IDefinitions, IDefinitionBuilder
{
    private readonly Dictionary<string, KeywordDefinition> _keywords = new();

    public KeywordDefinition? GetKeyword(string keyword)
    {
        return _keywords.TryGetValue(keyword, out var definition) ? definition : null;
    }

    public void AddKeyword(KeywordDefinition keywordDefinition)
    {
        if (_keywords.ContainsKey(keywordDefinition.Name))
            throw new InvalidOperationException($"Keyword ({keywordDefinition.Name}) already exists");
        
        _keywords.Add(keywordDefinition.Name, keywordDefinition);
    }
}