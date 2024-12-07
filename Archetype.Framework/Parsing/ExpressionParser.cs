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
    
    private IValue ParseEffectArgumentExpression(ReadExpression readExpression)
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
                throw new InvalidOperationException($"Not enough arguments for effect: {effectData.Keyword}: {nNonContextParameters} / {effectData.ArgumentExpressions.Length}");
            
            var argumentExpression = effectData.ArgumentExpressions[nNonContextParameters];
            
            effectParameters.Add(ParseEffectArgumentExpression(argumentExpression));
            
            nNonContextParameters++;
        }
        
        if (nNonContextParameters != effectData.ArgumentExpressions.Length)
            throw new InvalidOperationException($"Too many arguments for effect: {effectData.Keyword}: {nNonContextParameters} / {effectData.ArgumentExpressions.Length}");
        
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
        else if (type.Implements(typeof(IEnumerable)))
        {
            var itemType = type.GetElementType();
            
            if (itemType == typeof(int))
            {
                var atomValue = new AtomGroup<int?>(atomPath);
                INumber compareValue = int.TryParse(compareValueExpression, out var immediateValue) ?
                    new ImmediateNumber(immediateValue) :
                    new ReferenceNumber(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<int?>(atomValue, compareExpression, compareValue);
            }
            else if (itemType == typeof(string))
            {
                var atomValue = new AtomGroup<string>(atomPath);
                IWord compareValue = compareValueExpression.TryParseWord(out var word) ?
                    new ImmediateWord(word!) :
                    new ReferenceWord(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<string?>(atomValue, compareExpression, compareValue);
            }
            else if (itemType.Implements(typeof(IAtom)))
            {
                var atomValue = new AtomGroup<IAtom>(atomPath);
                var compareValue = new ReferenceAtom(compareValueExpression.Split('.'));
                
                return new AtomGroupPredicate<IAtom>(atomValue, compareExpression, compareValue);
            }
        }
        
        throw new InvalidOperationException($"Invalid type for predicate: {type}");
    }
    
    
}

