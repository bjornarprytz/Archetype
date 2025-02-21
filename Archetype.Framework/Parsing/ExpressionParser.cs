using System.Collections;
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
    
    public ICardProto ParseCard(CardData cardData)
    {
        return new CardProto()
        {
            Name = cardData.Name,
            Costs = cardData.Costs.ToDictionary(
                kvp => kvp.Key, 
                kvp => ParseCost(kvp.Key, kvp.Value)),
            Targets = cardData.Targets.Select(ParseTargets).ToArray(),
            
            Stats = cardData.Stats.ToDictionary(
                kvp => kvp.Key, 
                kvp => ParseNumberExpression(kvp.Value)),
            Facets = cardData.Facets,
            Tags = cardData.Tags,
            
            Effects = cardData.Effects.Select(ParseEffect).ToArray(),
            Variables = cardData.Variables.ToDictionary(
                kvp => kvp.Key, 
                kvp => ParseNumberExpression(kvp.Value)),
        };
    }
    
    private CostProto ParseCost(string resourceType, ReadExpression costExpression)
    {
        return new CostProto()
        {
            ResourceType = resourceType,
            Amount = ParseNumberExpression(costExpression),
        };
    }
    
    private TargetProto ParseTargets(TargetData targetData)
    {
        return new TargetProto()
        {
            Predicates = targetData.ConditionalExpressions.Select(ParsePredicate).ToArray(),
        };
    }

    private IValue<int?> ParseNumberExpression(ReadExpression readExpression)
    {
        if (int.TryParse(readExpression.Text, out var immediateValue))
        {
            return new ImmediateNumber(immediateValue);
        }
        
        return new Value<IResolutionContext, int?>(readExpression.Text.Split('.'));
    }
    
    private IValue<string> ParseWordExpression(ReadExpression readExpression)
    {
        if (readExpression.Text.TryParseWord(out var word))
        {
            return new ImmediateWord(word!);
        }
        
        return new Value<IResolutionContext, string>(readExpression.Text.Split('.'));
    }
    
    private IValue ParseUnknownValueExpression(ReadExpression readExpression)
    {
        var expression = readExpression.Text;
        
        if (int.TryParse(expression, out var immediateValue))
        {
            return new ImmediateNumber(immediateValue);
        }
        else if (expression.TryParseWord(out var word))
        {
            return new ImmediateWord(word!);
        }

        return expression.Split('.').CreateValueResolver<IResolutionContext>();
    }
    
    private EffectProto ParseEffect(EffectData effectData)
    {
        if (!_effectMethods.TryGetValue(effectData.Keyword, out var method))
            throw new InvalidOperationException($"Effect method not found: {effectData.Keyword}");

        var effectParameters = new List<IValue>();
        var nNonContextParameters = 0;
        
        foreach (var parameter in method.GetParameters())
        {
            var parameterType = parameter.ParameterType; 
            
            if (parameterType.Implements(typeof(IResolutionContext))) // The context gets injected automatically, if needed
            {
                continue;
            }
            
            if (nNonContextParameters >= effectData.ArgumentExpressions.Length)
                throw new InvalidOperationException($"Not enough arguments for effect: {effectData.Keyword}: {effectData.ArgumentExpressions.Length} / {nNonContextParameters+1}");
            
            var argumentExpression = effectData.ArgumentExpressions[nNonContextParameters];
            
            var valueExpression = ParseUnknownValueExpression(argumentExpression);
            
            if (!valueExpression.ValueType.Implements(parameterType.AsNullable()))
                throw new InvalidOperationException($"Invalid argument type for effect: {effectData.Keyword}: {valueExpression.ValueType} should implement {parameterType}");
            
            effectParameters.Add(valueExpression);
            
            nNonContextParameters++;
        }
        
        if (nNonContextParameters != effectData.ArgumentExpressions.Length)
            throw new InvalidOperationException($"Too many arguments for effect: {effectData.Keyword}: {effectData.ArgumentExpressions.Length} / {nNonContextParameters}");
        
        return new EffectProto()
        {
            Keyword = effectData.Keyword,
            Parameters = effectParameters,
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
            var atomValue = new Value<IAtom, int?>(atomPath);
            IValue<int?> compareValue = int.TryParse(compareValueExpression, out var immediateValue) ?
                new ImmediateNumber(immediateValue) :
                new Value<IAtom, int?>(compareValueExpression.Split('.'));
            
            return new AtomPredicate<IAtom, int?>(atomValue, compareExpression, compareValue);
        }
        else if (type == typeof(string))
        {
            var atomValue = new Value<IAtom, string>(atomPath);
            IValue<string> compareValue = compareValueExpression.TryParseWord(out var word) ?
                new ImmediateWord(word!) :
                new Value<IResolutionContext, string>(compareValueExpression.Split('.'));
            
            return new AtomPredicate<IAtom, string?>(atomValue, compareExpression, compareValue);
        }
        else if (type.Implements(typeof(IEnumerable)))
        {
            var itemType = type.GetItemsType();
            
            if (itemType == typeof(int))
            {
                var atomValue = new Group<IAtom, int?>(atomPath);
                IValue<int?> compareValue = int.TryParse(compareValueExpression, out var immediateValue) ?
                    new ImmediateNumber(immediateValue) :
                    new Value<IResolutionContext, int?>(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<IAtom, int?>(atomValue, compareExpression, compareValue);
            }
            else if (itemType == typeof(string))
            {
                var atomValue = new Group<IAtom, string>(atomPath);
                IValue<string> compareValue = compareValueExpression.TryParseWord(out var word) ?
                    new ImmediateWord(word!) :
                    new Value<IAtom, string>(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<IAtom, string?>(atomValue, compareExpression, compareValue);
            }
            else if (itemType.Implements(typeof(IAtom)))
            {
                var atomValue = new Group<IAtom, IAtom>(atomPath);
                var compareValue = new Value<IResolutionContext, IAtom>(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<IAtom, IAtom>(atomValue, compareExpression, compareValue);
            }
        }
        
        throw new InvalidOperationException($"Invalid type for predicate: {type}");
    }
    
    
}

