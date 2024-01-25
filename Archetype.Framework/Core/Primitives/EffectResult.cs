using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IEffectResult { }

public interface IAllowedTargets : IEffectResult
{
    public IReadOnlyList<IAtom> Options { get; }
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
    public static IEffectResult NoOp { get; } = new NoOpResult();
    public static IEffectResult Failed { get; } = new FailureResult();
}

public record NoOpResult : IEffectResult { }
public record FailureResult : IEffectResult { }

public record KeywordFrame : IKeywordFrame
{
    private KeywordFrame(params IKeywordInstance[] effects)
    {
        Effects = effects;
    }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
    public bool IsNoOp { get; } = false;
    public bool Failed { get; } = false;
    
    public static IKeywordFrame Compose(params IKeywordInstance[] effects)
    {
        return new KeywordFrame(effects);
    }

}

public record PromptDescription(Guid PromptId, IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks,
    string PromptText) : IPromptDescription;

public record AllowedTargets(IReadOnlyList<IAtom> Options) : IAllowedTargets;
