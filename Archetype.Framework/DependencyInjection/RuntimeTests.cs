using System.Reflection;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Meta;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Framework.DependencyInjection;

internal static class RuntimeTests
{
    public static void CheckRulesIntegrity(this IServiceProvider serviceProvider)
    {
        var rules = serviceProvider.GetRequiredService<IRules>();
        
        var keywordDefinitions = rules.Keywords.ToList();
        
        keywordDefinitions.EnsureKeywordAttribute();
        keywordDefinitions.EnsureNoDuplicates();
    }
    
    private static void EnsureNoDuplicates(this IEnumerable<IKeywordDefinition> keywordDefinitions)
    {
        var duplicates = keywordDefinitions.Select(d => d.GetType())
            .GroupBy(t => t.GetCustomAttribute<KeywordAttribute>()?.Keyword)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        if (duplicates.Count != 0)
            throw new InvalidOperationException($"Duplicate keywords found: {string.Join(", ", duplicates)}");
    }
    
    private static void EnsureKeywordAttribute(this IEnumerable<IKeywordDefinition> keywordDefinitions)
    {
        var keywordDefinitionsWithoutKeywordAttribute = keywordDefinitions
            .Select(d => d.GetType())
            .Where(t => string.IsNullOrWhiteSpace(t.GetCustomAttribute<KeywordAttribute>()?.Keyword))
            .ToList();
        
        if (keywordDefinitionsWithoutKeywordAttribute.Count != 0)
            throw new InvalidOperationException($"Keyword definitions without {nameof(KeywordAttribute)}: {string.Join(", ", keywordDefinitionsWithoutKeywordAttribute)}");
    }
}