using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.Framework.Parsing;

public static class Extensions
{
    public static KeywordOperand ToOperand(this ActionBlockParser.OperandContext context)
    {
        Func<IResolutionContext, object> func;
        
        if (context.any() is { } anyToken)
        {
            if (anyToken.NUMBER() is { } numberToken)
            {
                var number = int.Parse(numberToken.GetText());
                func = _ => number;
            }
            else if (anyToken.WORD() is { } stringToken)
            {
                var word = stringToken.GetText();
                func = _ => word;
            }
            else
            {
                throw new InvalidOperationException($"Could not parse any: {context.GetText()}");
            }
        }
        else if (context.STRING() is { } stringToken)
        {
            var str = stringToken.GetText();
            func = _ => str;
        }
        else if (context.computedValueRef() is { } computedValueRef && computedValueRef.index() is { } indexContext)
        {
            var index = int.Parse(indexContext.GetText());
            
            func = ctx => ctx.ComputedValues[index];
        }
        else if (context.filter() is { } filterContext)
        {
            var filter = Filter.Parse(filterContext.GetText());
            func = ctx => filter.ProvideAtoms(ctx);
        }
        else
        {
            throw new InvalidOperationException($"Could not parse operand: {context.GetText()}");
        }
        
        return new KeywordOperand
        {
            GetValue = func
        };
    }
    
    public static KeywordInstance GetKeywordInstance(this ActionBlockParser.KeywordExpressionContext keywordContext, IDefinitions definitions)
    {
        if (keywordContext.keyword().GetText() is not { } keyword 
            || definitions.GetDefinition(keyword) is not { } definition)
            throw new InvalidOperationException("Keyword expression has no valid keyword");
        
        var targetRefs = keywordContext.targetRef()?.Select(GetTarget).ToList() ?? new List<KeywordTarget>();
        var operands = keywordContext.operand()?.Select(ToOperand).ToList() ?? new List<KeywordOperand>();

        return new KeywordInstance
        {
            Keyword = keyword,
            Operands = operands,
            Targets = targetRefs
        };
    }

    private static IEnumerable<KeywordInstance> GetKeywordInstances(this IEnumerable<ActionBlockParser.KeywordExpressionContext>? contexts, IDefinitions definitions)
    {
        return contexts?.Select(kw => kw.GetKeywordInstance(definitions)) ?? new List<KeywordInstance>();
    }

    public static IEnumerable<TargetDescription> GetTargetSpecs(this ActionBlockParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext.targets()?.targetSpecs()?
            .Select(c => new TargetDescription(Filter.Parse(c.filters().GetText()), c.OPTIONAL() != null))
            ?? new List<TargetDescription>();
    } 

    public static IEnumerable<KeywordInstance> GetComputedValues(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.computedValues()?
            .keywordExpression().GetKeywordInstances(definitions) 
            ?? new List<KeywordInstance>();
    }
    
    public static IEnumerable<KeywordInstance> GetCosts(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.costs()?
            .keywordExpression().GetKeywordInstances(definitions)
            ?? new List<KeywordInstance>();
    }
    
    public static IEnumerable<KeywordInstance> GetConditions(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        return actionBlockContext.conditions()?
            .keywordExpression().GetKeywordInstances(definitions)
            ?? new List<KeywordInstance>();
    }

    public static IEnumerable<KeywordInstance> GetEffectKeywordInstances(
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
            return new KeywordTarget(
                ctx => ctx.Targets[index]
            );
        }
        if (targetText == "~")
        {
            return new KeywordTarget(
                ctx => ctx.Source
            );
        }
        
        throw new InvalidOperationException($"Could not parse target ref: {targetText}");
        
    }
}