using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Parsing;

public static class Extensions
{
    public static KeywordOperand ToOperand(this ActionBlockParser.OperandContext context)
    {
        if (context.any() is { } anyToken)
        {
            if (anyToken.NUMBER() is { } numberToken)
            {
                var number = int.Parse(numberToken.GetText());
                
                return new KeywordOperand<int>(_ => number);
            }

            if (anyToken.WORD() is { } stringToken)
            {
                var word = stringToken.GetText();
                
                return new KeywordOperand<string>(_ => word);
            }
            throw new InvalidOperationException($"Could not parse any: {context.GetText()}");
        }

        {
            if (context.STRING() is { } stringToken)
            {
                var str = stringToken.GetText();

                return new KeywordOperand<string>(_ => str);
            }
            if (context.computedValueRef() is { } computedValueRef && computedValueRef.index() is { } indexContext)
            {
                var index = int.Parse(indexContext.GetText());

                return new KeywordOperand<int>(ctx => ctx.ComputedValues[index]);
            }
            if (context.filter() is { } filterContext)
            {
                var filter = Filter.Parse(filterContext.GetText());

                return new KeywordOperand<IEnumerable<IAtom>>(ctx => filter.ProvideAtoms(ctx));
            }
        }

        throw new InvalidOperationException($"Could not parse operand: {context.GetText()}");
    }
    
    public static IKeywordInstance GetKeywordInstance(this ActionBlockParser.KeywordExpressionContext keywordContext, IDefinitions definitions)
    {
        if (keywordContext.keyword().GetText() is not { } keyword 
            || definitions.GetDefinition(keyword) is not { } definition)
            throw new InvalidOperationException("Keyword expression has no valid keyword");
        
        var targetRefs = keywordContext.targetRef()?.Select(GetTarget).ToList() ?? new List<KeywordTarget>();
        var operands = keywordContext.operand()?.Select(ToOperand).ToList() ?? new List<KeywordOperand>();

        return definition.CreateInstance(operands, targetRefs);
    }

    private static IEnumerable<IKeywordInstance> GetKeywordInstances(this IEnumerable<ActionBlockParser.KeywordExpressionContext>? contexts, IDefinitions definitions)
    {
        return contexts?.Select(kw => kw.GetKeywordInstance(definitions)) ?? new List<IKeywordInstance>();
    }

    public static IEnumerable<CardTargetDescription> GetTargetSpecs(this ActionBlockParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext.targets()?.targetSpecs()?
            .Select(c => new CardTargetDescription(Filter.Parse(c.filters().GetText()), c.OPTIONAL() != null))
            ?? new List<CardTargetDescription>();
    }

    public static IEnumerable<IKeywordInstance> GetComputedValues(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.computedValues()?
            .keywordExpression().GetKeywordInstances(definitions) 
            ?? new List<IKeywordInstance>();
    }
    
    public static IEnumerable<IKeywordInstance> GetCosts(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.costs()?
            .keywordExpression().GetKeywordInstances(definitions)
            ?? new List<IKeywordInstance>();
    }
    
    public static IEnumerable<IKeywordInstance> GetConditions(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.conditions()?
            .keywordExpression().GetKeywordInstances(definitions)
            ?? new List<IKeywordInstance>();
    }

    public static IEnumerable<IKeywordInstance> GetEffectKeywordInstances(
        this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.keywordExpression()
            .GetKeywordInstances(definitions);
    }

    private static KeywordTarget GetTarget(this ActionBlockParser.TargetRefContext targetRefContext)
    {
        var targetText =  targetRefContext.index()?.NUMBER()?.GetText() ?? targetRefContext.SELF()?.GetText() ?? throw new InvalidOperationException("Target ref has no index or SELF");
        
        if (int.TryParse(targetText, out var index))
        {
            return Declare.Target(index);
        }
        if (targetText == "~")
        {
            return Declare.TargetSource();
        }
        
        throw new InvalidOperationException($"Could not parse target ref: {targetText}");
        
    }
}