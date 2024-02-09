using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Extensions;

public static class RulesExtensions
{
    public static TDef GetOrThrow<TDef>(this IRules rules, IKeywordInstance keywordInstance) where TDef : IKeywordDefinition
    {
        return rules.GetOrThrow<TDef>(keywordInstance.Keyword);
    }
    
    public static IKeywordDefinition? GetOrThrow(this IRules rules, IKeywordInstance keywordInstance)
    {
        return rules.GetDefinition(keywordInstance.Keyword) ?? throw new InvalidOperationException($"Keyword ({keywordInstance.Keyword}) not found");
    }
    
    public static TDef GetOrThrow<TDef>(this IRules rules, string keyword) where TDef : IKeywordDefinition
    {
        var r = rules.GetDefinition(keyword);
        
        if (r is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    
    public static TDef GetOrThrow<TDef>(this IRules rules) where TDef : IKeywordDefinition
    {
        if (rules.GetDefinition<TDef>() is not { } requiredDefinition)
            throw new InvalidOperationException($"There is no definition for {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
}