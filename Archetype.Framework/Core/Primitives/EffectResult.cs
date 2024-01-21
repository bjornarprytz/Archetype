namespace Archetype.Framework.Core.Primitives;

public interface IEffectResult
{
    bool IsNoOp { get; }
}


public interface IKeywordFrame : IEffectResult
{   
    public IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record EffectResult : IEffectResult
{
    public bool IsNoOp { get; private init; } = false;
    public static IEffectResult Resolved { get; } = new EffectResult();
    public static IEffectResult NoOp { get; } = new EffectResult { IsNoOp = true };
}

public record KeywordFrame : IKeywordFrame
{
    private KeywordFrame(params IKeywordInstance[] effects)
    {
        Effects = effects;
    }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
    public bool IsNoOp { get; } = false;
    
    public static IKeywordFrame Compose(params IKeywordInstance[] effects)
    {
        return new KeywordFrame(effects);
    }

}