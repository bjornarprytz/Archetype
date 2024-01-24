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
        
        keywordDefinitions.EnsureNoDuplicates();
        keywordDefinitions.EnsureOperandDeclarations();
    }
    
    private static void EnsureNoDuplicates(this IEnumerable<IKeywordDefinition> keywordDefinitions)
    {
        var duplicates = keywordDefinitions
            .GroupBy(def => def.Id)
            .Where(g => g.Count() > 1)
            .ToList();
        
        if (duplicates.Count != 0)
            throw new InvalidOperationException($"Duplicate keywords found: {string.Join(", ", duplicates.Select(pair => $"{pair.Key} > {string.Join("|", pair.Select(p => p.GetType()))} "))}");
    }
    
    private static void EnsureOperandDeclarations(this IEnumerable<IKeywordDefinition> keywordDefinitions)
    {
        foreach (var definition in keywordDefinitions)
        {
            if (definition.GetType().GetCustomAttribute<KeywordAttribute>() is not { } attribute) 
                continue;
            
            if (attributeOperandDeclaration != definition.GetType())
                throw new InvalidOperationException($"Operand declaration mismatch: {definition.GetType()} != {attributeOperandDeclaration}");
        }
    }
}