using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;

public class EffectDefinition(Delegate handler) : IEffectDefinition
{
    public string Keyword { get; } = handler.Method.Name;

    public IEffectResult Resolve(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        if (Keyword != keywordInstance.Keyword)
            throw new InvalidOperationException(
                $"KeywordInstance ({keywordInstance.Keyword}) does not match KeywordDefinition ({Keyword})");

        var parameters = new object?[keywordInstance.Operands.Count + 1];

        var i = 0;
        parameters[i] = context;
        
        foreach (var arg in keywordInstance.Operands.Select(o => o.GetValue(context)))
        {
            i++;
            parameters[i] = arg;
        }
        
        
                
        if (handler.DynamicInvoke(parameters) is IEffectResult result)
            return result;
        
        throw new InvalidOperationException($"KeywordDefinition ({Keyword}) did not return an {nameof(IEffectResult)}");
    }
}


