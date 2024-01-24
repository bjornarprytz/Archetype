using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IEffectResult
{
    bool IsNoOp { get; }
}


public interface IPromptDescription : IEffectResult
{
    public Guid PromptId { get; }
    public IReadOnlyList<Guid> Options { get; }
    public int MinPicks { get; }
    public int MaxPicks { get; }
    public string PromptText { get; }
}

// Composition of effects (non-leaf nodes)
public interface IKeywordFrame : IEffectResult
{   
    public IReadOnlyList<IKeywordInstance> Effects { get; }
}

// Execution of state changes (leaf nodes)
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

public record PromptDescription(Guid PromptId, IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : IPromptDescription
{
    public bool IsNoOp => false;
}