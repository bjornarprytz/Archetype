using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="StaticEffectDef"/> instances.
/// <example>
/// <code>
/// // A permanent aura that contributes a state modification:
/// var aura = new StaticEffectDefBuilder()
///     .Permanent()
///     .WithStateContribution(b => b.Step("modify-accumulator", Kw.Param("source"), Kw.Str("attack"), Kw.Num(2)))
///     .Build();
///
/// // An effect that expires after 2 turns and fires a trigger:
/// var buff = new StaticEffectDefBuilder()
///     .ForTurns(2)
///     .WithTrigger(new TriggerDefinitionBuilder("take-damage", TriggerScope.ThisAction)
///         .OnFired(b => b.Step("modify-accumulator", Kw.Param("source"), Kw.Str("shield"), Kw.Num(-1)))
///         .Build())
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class StaticEffectDefBuilder
{
    private LifetimeSpec _lifetime = LifetimeSpec.Permanent;
    private EffectBlockDef? _stateContributionBlock;
    private TriggerDefinition? _trigger;
    private ParameterModification? _parameterModification;

    /// <summary>Sets a permanent lifetime (never expires automatically).</summary>
    public StaticEffectDefBuilder Permanent()
    {
        _lifetime = LifetimeSpec.Permanent;
        return this;
    }

    /// <summary>Sets a lifetime that expires after <paramref name="turns"/> turns.</summary>
    public StaticEffectDefBuilder ForTurns(int turns)
    {
        _lifetime = new LifetimeSpec([new TurnTimer(turns)]);
        return this;
    }

    /// <summary>Sets a lifetime that expires when <paramref name="expression"/> is false.</summary>
    public StaticEffectDefBuilder While(KeywordNode expression)
    {
        _lifetime = new LifetimeSpec([new WhileCondition(expression)]);
        return this;
    }

    /// <summary>Sets a fully specified <see cref="LifetimeSpec"/> directly.</summary>
    public StaticEffectDefBuilder WithLifetime(LifetimeSpec lifetime)
    {
        _lifetime = lifetime;
        return this;
    }

    /// <summary>Sets the state-contribution block directly.</summary>
    public StaticEffectDefBuilder WithStateContribution(EffectBlockDef block)
    {
        _stateContributionBlock = block;
        return this;
    }

    /// <summary>Builds and sets the state-contribution block via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public StaticEffectDefBuilder WithStateContribution(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _stateContributionBlock = builder.Build();
        return this;
    }

    /// <summary>Sets the trigger definition.</summary>
    public StaticEffectDefBuilder WithTrigger(TriggerDefinition trigger)
    {
        _trigger = trigger;
        return this;
    }

    /// <summary>Sets a parameter modification for this static effect.</summary>
    public StaticEffectDefBuilder WithParameterModification(ParameterModification modification)
    {
        _parameterModification = modification;
        return this;
    }

    /// <summary>Builds and returns the <see cref="StaticEffectDef"/>.</summary>
    public StaticEffectDef Build() => new(
        Lifetime:               _lifetime,
        StateContributionBlock: _stateContributionBlock,
        Trigger:                _trigger,
        ParameterModification:  _parameterModification);
}
