using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="EffectBlockDef"/> instances.
/// <example>
/// <code>
/// var block = new EffectBlockBuilder()
///     .Step("modify-accumulator", Kw.Param("target"), Kw.Str("health"), Kw.Num(-1))
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class EffectBlockBuilder
{
    private readonly List<EffectBlockStep> _steps = new();

    /// <summary>
    /// Appends a step that invokes <paramref name="keyword"/> with the given arguments.
    /// </summary>
    public EffectBlockBuilder Step(string keyword, params KeywordNode[] args)
    {
        _steps.Add(new EffectBlockStep(keyword, args));
        return this;
    }

    /// <summary>
    /// Appends a step that invokes <paramref name="keyword"/> and binds the return value
    /// to <paramref name="bindTo"/> so that later steps can reference it by name.
    /// </summary>
    public EffectBlockBuilder Step(string keyword, string bindTo, params KeywordNode[] args)
    {
        _steps.Add(new EffectBlockStep(keyword, args, bindTo));
        return this;
    }

    /// <summary>Builds and returns the <see cref="EffectBlockDef"/>.</summary>
    public EffectBlockDef Build() => new(_steps);
}
