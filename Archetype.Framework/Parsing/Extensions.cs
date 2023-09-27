using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.Framework.Parsing;

public static class Extensions
{
    public static KeywordOperand ToOperand(this ActionBlockParser.OperandContext context)
    {
        var text = context.GetText();

        Func<IResolutionContext, object> func;
        
        if (context.NUMBER() is { } numberToken)
        {
            var number = int.Parse(numberToken.GetText());
            func = _ => number;
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
            func = ctx => filter.ProvideAtoms(ctx); // TODO: Is this the right thing to return? Or should the caller use the filter
        }
        else
        {
            throw new InvalidOperationException($"Could not parse operand: {text}");
        }
        
        return new KeywordOperand
        {
            GetValue = func
        };
    }
    
    public static KeywordInstance GetKeywordInstance(this ActionBlockParser.KeywordExpressionContext keywordContext, IDefinitions definitions)
    {
        if (keywordContext.keyword().GetText() is not { } keyword)
            throw new InvalidOperationException("Keyword expression has no keyword");
        
        var targetRefs = keywordContext.targetRef()?.Select(GetTargetIndex).ToList() ?? new List<string>();
        
        var operands = keywordContext.operand()


        return definitions.GetDefinition() ?? throw new InvalidOperationException($"Could not find keyword definition: {name}");
    }
    
    public static IEnumerable<KeywordTarget> GetTargetSpecs(this ActionBlockParser.ActionBlockContext actionBlockContext)
    {
        var targetDescriptionContext = actionBlockContext.targets().targetSpecs();
        
        return keywordContext.targetRef()?.Select(GetTargetDescription) ?? Enumerable.Empty<TargetDescription>();
    } 

    public static IEnumerable<ComputedValueInstance> GetComputedValues(this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        var computedValueContext = actionBlockContext.computedValues().keywordExpression().Select(kw => kw.GetKeywordInstance(definitions));
        
        return keywordContext.targetRef()?.Select(GetTargetDescription) ?? Enumerable.Empty<TargetDescription>();
    }

    public static IEnumerable<EffectInstance> GetEffectKeywordInstances(
        this ActionBlockParser.ActionBlockContext actionBlockContext, IDefinitions definitions)
    {
        var keywords = actionBlockContext.keywordExpression().Select(k => k.GetKeywordInstance(definitions)).ToList();
        
        var effectKeyword = keywords.OfType<EffectInstance>().ToList();
        
        if (keywords.Count != effectKeyword.Count)
            throw new InvalidOperationException("Not all keywords in the action block are effects");
        
        return effectKeyword;
    }

    private static string GetTargetIndex(this ActionBlockParser.TargetRefContext targetRefContext)
    {
        return targetRefContext.index()?.NUMBER()?.GetText() ?? targetRefContext.SELF()?.GetText() ?? throw new InvalidOperationException("Target ref has no index or SELF");
    }
}