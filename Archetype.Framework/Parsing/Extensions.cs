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
        else
        {
            throw new InvalidOperationException($"Could not parse operand: {text}");
        }
        
        return new KeywordOperand
        {
            GetValue = func
        };
    }
}