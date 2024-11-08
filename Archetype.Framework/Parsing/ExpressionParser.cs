using Archetype.Framework.Data;
using Archetype.Framework.Effects;
using Archetype.Framework.Resolution;

namespace Archetype.Framework.Parsing;

public static class ExpressionParser
{
    public static Func<TInput, TOutput> ParseFunc<TInput, TOutput>(Expression expression)
    {
        // TODO: Walk the expression and extract the value from the input object
        
        throw new NotImplementedException();
    }

    public static Func<ResolutionContext, IEffectResult> ParseEffect(Expression expression)
    {
        throw new NotImplementedException();
    }
}