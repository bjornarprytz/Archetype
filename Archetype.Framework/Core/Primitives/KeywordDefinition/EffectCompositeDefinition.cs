namespace Archetype.Framework.Core.Primitives;

public interface IEffectCompositeDefinition : IKeywordDefinition
{
    IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload);
}

public abstract class EffectCompositeDefinition : KeywordDefinition, IEffectCompositeDefinition
{
    public abstract IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload);
}