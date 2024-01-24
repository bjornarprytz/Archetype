using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Id { get; }
    IEffectResult Resolve(IResolutionContext context, EffectPayload payload);
}


public class KeywordDefinition<T> : IKeywordDefinition
{
    private Delegate _handler;
    public KeywordDefinition(Delegate handler)
    {
        Id = handler.Method.Name;
        _handler = handler;

        var parameters = handler.Method.GetParameters();
    }
    public string Id { get; }
    public IEffectResult Resolve(IResolutionContext context, EffectPayload payload)
    {
        if (Id != payload.EffectId)
            throw new InvalidOperationException(
                $"KeywordInstance ({payload.EffectId}) does not match KeywordDefinition ({Id})");

        var parameters = new List<object>
            { context }
            .Append(payload.Operands)
            .ToArray();
                
        if (_handler.DynamicInvoke(parameters) is IEffectResult result)
            return result;
        
        throw new InvalidOperationException($"KeywordDefinition ({Id}) did not return an {nameof(IEffectResult)}");
    }
}


