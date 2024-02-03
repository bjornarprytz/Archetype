using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;

public class EffectDefinition<T>(Delegate handler) : IEffectDefinition
{
    public string Keyword { get; } = handler.Method.Name;

    public IEffectResult Resolve(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        if (Keyword != keywordInstance.Keyword)
            throw new InvalidOperationException(
                $"KeywordInstance ({keywordInstance.Keyword}) does not match KeywordDefinition ({Keyword})");

        var parameters = new List<object>
            { context }
            .Append(keywordInstance.Operands.Select(o => o.GetValue(context)))
            .ToArray();
                
        if (handler.DynamicInvoke(parameters) is IEffectResult result)
            return result;
        
        throw new InvalidOperationException($"KeywordDefinition ({Keyword}) did not return an {nameof(IEffectResult)}");
    }
}


