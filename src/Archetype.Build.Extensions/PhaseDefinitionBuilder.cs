using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="PhaseDefinition"/> instances.
/// <example>
/// <code>
/// var phase = new PhaseDefinitionBuilder("main")
///     .WithInit(b => b.Step("draw-card", Kw.Num(1)))
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class PhaseDefinitionBuilder
{
    private readonly string _name;
    private EffectBlockDef? _init;
    private EffectBlockDef? _cleanup;

    /// <summary>Initialises a builder for a phase with the given <paramref name="name"/>.</summary>
    public PhaseDefinitionBuilder(string name) => _name = name;

    /// <summary>Sets the effect block that runs at the start of this phase.</summary>
    public PhaseDefinitionBuilder WithInit(EffectBlockDef init)
    {
        _init = init;
        return this;
    }

    /// <summary>Builds and sets the init block via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public PhaseDefinitionBuilder WithInit(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _init = builder.Build();
        return this;
    }

    /// <summary>Sets the effect block that runs at the end of this phase.</summary>
    public PhaseDefinitionBuilder WithCleanup(EffectBlockDef cleanup)
    {
        _cleanup = cleanup;
        return this;
    }

    /// <summary>Builds and sets the cleanup block via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public PhaseDefinitionBuilder WithCleanup(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _cleanup = builder.Build();
        return this;
    }

    /// <summary>Builds and returns the <see cref="PhaseDefinition"/>.</summary>
    public PhaseDefinition Build() => new(_name, _init, _cleanup);
}
