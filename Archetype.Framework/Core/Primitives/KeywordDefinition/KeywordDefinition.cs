using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Id { get; }
    IEffectResult Resolve(IResolutionContext context, IKeywordInstance keywordInstance);
}


public class KeywordDefinition<T>(Delegate handler) : IKeywordDefinition
{
    public string Id { get; } = handler.Method.Name;

    public IEffectResult Resolve(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        if (Id != keywordInstance.Keyword)
            throw new InvalidOperationException(
                $"KeywordInstance ({keywordInstance.Keyword}) does not match KeywordDefinition ({Id})");

        var parameters = new List<object>
            { context }
            .Append(keywordInstance.Operands.Select(o => o.GetValue(context)))
            .ToArray();
                
        if (handler.DynamicInvoke(parameters) is IEffectResult result)
            return result;
        
        throw new InvalidOperationException($"KeywordDefinition ({Id}) did not return an {nameof(IEffectResult)}");
    }
}


