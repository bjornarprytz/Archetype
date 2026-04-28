using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="NamedEffectBlockDef"/> instances (activated abilities).
/// <example>
/// <code>
/// var ability = new NamedEffectBlockDefBuilder("regenerate")
///     .WithActivationCondition(Kw.AtLeast(Kw.GetState(Kw.Param("source"), Kw.Str("health")), Kw.Num(1)))
///     .WithBody(b => b.Step("modify-accumulator", Kw.Param("source"), Kw.Str("health"), Kw.Num(2)))
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class NamedEffectBlockDefBuilder
{
    private readonly string _name;
    private KeywordNode? _activationCondition;
    private readonly List<CostDef> _costs = new();
    private EffectBlockDef _body = EffectBlockDef.Empty;

    /// <summary>Initialises a builder for a named ability with the given <paramref name="name"/>.</summary>
    public NamedEffectBlockDefBuilder(string name) => _name = name;

    /// <summary>Sets the guard expression evaluated before offering this ability.</summary>
    public NamedEffectBlockDefBuilder WithActivationCondition(KeywordNode condition)
    {
        _activationCondition = condition;
        return this;
    }

    /// <summary>Adds a cost that must be paid before the body executes.</summary>
    public NamedEffectBlockDefBuilder AddCost(CostDef cost)
    {
        _costs.Add(cost);
        return this;
    }

    /// <summary>Sets the effect block body directly.</summary>
    public NamedEffectBlockDefBuilder WithBody(EffectBlockDef body)
    {
        _body = body;
        return this;
    }

    /// <summary>Builds and sets the body via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public NamedEffectBlockDefBuilder WithBody(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _body = builder.Build();
        return this;
    }

    /// <summary>Builds and returns the <see cref="NamedEffectBlockDef"/>.</summary>
    public NamedEffectBlockDef Build() => new(
        Name:                _name,
        ActivationCondition: _activationCondition,
        Cost:                _costs.Count > 0 ? _costs : null,
        Body:                _body);
}
