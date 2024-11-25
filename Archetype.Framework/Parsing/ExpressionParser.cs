using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Archetype.Framework.Core;
using Archetype.Framework.Data;
using Archetype.Framework.Effects;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

public class ExpressionParser
{
    // Keyword -> Method
    private readonly Dictionary<string, MethodInfo> _effectMethods;

    public ExpressionParser(IEnumerable<MethodInfo> effectMethods)
    {
        _effectMethods = effectMethods.ToDictionary(m => m.GetRequiredAttribute<EffectAttribute>().Keyword);
    }
    
    public CardProto ParseCard(CardData cardData)
    {
        return new CardProto()
        {
            Name = cardData.Name,
            Costs = cardData.Costs.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
            Targets = cardData.Targets.Select(ParseTargets).ToArray(),
            
            Stats = cardData.Stats.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
            Facets = cardData.Facets,
            Tags = cardData.Tags,
            
            Effects = cardData.Effects.Select(ParseEffect).ToArray(),
            Variables = cardData.Variables.ToDictionary(kvp => kvp.Key, kvp => ParseNumberExpression(kvp.Value)),
        };
    }
    
    private TargetProto ParseTargets(TargetData targetData)
    {
        return new TargetProto()
        {
            Predicates = targetData.ConditionalExpressions.Select(ParsePredicate).ToArray(),
        };
    }

    private INumber ParseNumberExpression(ReadExpression readExpression)
    {
        if (int.TryParse(readExpression.Text, out var immediateValue))
        {
            return new ImmediateNumber(immediateValue);
        }
        
        return new ReferenceNumber(ParsePathExpression(readExpression.Text));
    }
    
    private string[] ParsePathExpression(string expression)
    {
        return expression.Split('.');
    }

    private EffectProto ParseEffect(EffectData effectData)
    {
        return new EffectProto()
        {
            Keyword = effectData.Keyword,
            Parameters = effectData.ArgumentExpressions.Select(ParseNumberExpression).ToArray(),
        };
    }
    
    private IAtomPredicate ParsePredicate(ReadExpression readExpression)
    {
        var parts = readExpression.Text.Split(' ').Select(t => t.Trim()).ToArray();

        if (parts.Length != 3)
            throw new InvalidOperationException($"Invalid predicate: {readExpression.Text}");

        var atomValue = new AtomValue(parts[0].Split('.'));
        
        var comparisonOperator = ParseComparisonExpression(parts[1]);

        var compareValue = new ReferenceValue(parts[2].Split('.'));
        
        return new AtomPredicate(atomValue, comparisonOperator, compareValue);
    }
    
    private static ComparisonOperator ParseComparisonExpression(string text)
    {
        return text switch
        {
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "=" => ComparisonOperator.Equal,
            "!=" => ComparisonOperator.NotEqual,
            "has" => ComparisonOperator.Contains,
            "!has" => ComparisonOperator.NotContains,
            _ => throw new InvalidOperationException($"Unknown comparison operator: {text}")
        };
    }
}

