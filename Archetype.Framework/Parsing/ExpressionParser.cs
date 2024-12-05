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
        
        return new ReferenceNumber(readExpression.Text.Split('.'));
    }
    
    private IWord ParseWordExpression(ReadExpression readExpression)
    {
        if (readExpression.Text.TryParseWord(out var word))
        {
            return new ImmediateWord(word!);
        }
        
        return new ReferenceWord(readExpression.Text.Split('.'));
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
        var expressions = readExpression.Text.Split(' ').Select(t => t.Trim()).ToArray();

        if (expressions.Length != 3)
            throw new InvalidOperationException($"Invalid predicate: {readExpression.Text}");

        var atomPath = expressions[0].Split('.');
        var compareExpression = expressions[1];
        var compareValueExpression = expressions[2];
        
        var type = atomPath.GetValueType<IAtom>();
        
        if (type == typeof(int))
        {
            var atomValue = new AtomNumber(atomPath);
            INumber compareValue = int.TryParse(compareValueExpression, out var immediateValue) ?
                new ImmediateNumber(immediateValue) :
                new ReferenceNumber(compareValueExpression.Split('.'));
            
            return new AtomPredicate<int?>(atomValue, compareExpression, compareValue);
        }
        else if (type == typeof(string))
        {
            var atomValue = new AtomWord(atomPath);
            IWord compareValue = compareValueExpression.TryParseWord(out var word) ?
                new ImmediateWord(word!) :
                new ReferenceWord(compareValueExpression.Split('.'));
            
            return new AtomPredicate<string?>(atomValue, compareExpression, compareValue);
        }
        else if (type == typeof(IEnumerable<IAtom>))
        {
            
            var atomValue = new AtomGroup(atomPath);
            var compareValue = new ReferenceGroup(compareValueExpression.Split('.'));
            
            return new AtomPredicate<IEnumerable<IAtom>?>(atomValue, compareExpression, compareValue);
        }
        
        throw new InvalidOperationException($"Invalid type for predicate: {type}");
    }
    
    
}

