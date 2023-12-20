namespace Archetype.Framework.Core.Primitives;

public interface IEffectPrimitiveDefinition : IKeywordDefinition
{
    public IEvent Resolve(IResolutionContext context, EffectPayload effectPayload);
}


public abstract class EffectPrimitiveDefinition : KeywordDefinition, IEffectPrimitiveDefinition
{
    public abstract IEvent Resolve(IResolutionContext context, EffectPayload effectPayload);
}