using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="CardDefinition"/> instances.
/// <example>
/// <code>
/// var card = new CardDefinitionBuilder("Strike")
///     .WithStaticProperty("flavour", "A swift attack.")
///     .WithStateField("health", StateFieldType.Number)
///     .WithPrimaryEffect(b => b
///         .Step("modify-accumulator", Kw.Param("target"), Kw.Str("health"), Kw.Num(-1)))
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class CardDefinitionBuilder
{
    private readonly string _name;
    private readonly Dictionary<string, object> _staticProperties = new();
    private EffectBlockDef _primaryEffect = EffectBlockDef.Empty;
    private readonly List<NamedEffectBlockDef> _additionalEffects = new();
    private readonly List<StaticEffectDef> _staticEffects = new();
    private KeywordNode? _activationCondition;
    private readonly List<CostDef> _costs = new();
    private List<StateFieldDecl>? _stateMap;

    /// <summary>Initialises a builder for a card with the given <paramref name="name"/>.</summary>
    public CardDefinitionBuilder(string name) => _name = name;

    /// <summary>Adds or overwrites a static property on this card definition.</summary>
    public CardDefinitionBuilder WithStaticProperty(string key, object value)
    {
        _staticProperties[key] = value;
        return this;
    }

    /// <summary>Sets the primary effect block directly.</summary>
    public CardDefinitionBuilder WithPrimaryEffect(EffectBlockDef effect)
    {
        _primaryEffect = effect;
        return this;
    }

    /// <summary>Builds and sets the primary effect block via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public CardDefinitionBuilder WithPrimaryEffect(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _primaryEffect = builder.Build();
        return this;
    }

    /// <summary>Adds a named activatable effect (e.g. an activated ability).</summary>
    public CardDefinitionBuilder AddEffect(NamedEffectBlockDef effect)
    {
        _additionalEffects.Add(effect);
        return this;
    }

    /// <summary>Builds and adds a named activatable effect via a <see cref="NamedEffectBlockDefBuilder"/> callback.</summary>
    public CardDefinitionBuilder AddEffect(string name, Action<NamedEffectBlockDefBuilder> configure)
    {
        var builder = new NamedEffectBlockDefBuilder(name);
        configure(builder);
        _additionalEffects.Add(builder.Build());
        return this;
    }

    /// <summary>Adds a static effect definition.</summary>
    public CardDefinitionBuilder AddStaticEffect(StaticEffectDef effect)
    {
        _staticEffects.Add(effect);
        return this;
    }

    /// <summary>Builds and adds a static effect via a <see cref="StaticEffectDefBuilder"/> callback.</summary>
    public CardDefinitionBuilder AddStaticEffect(Action<StaticEffectDefBuilder> configure)
    {
        var builder = new StaticEffectDefBuilder();
        configure(builder);
        _staticEffects.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Sets the activation condition guard evaluated before offering <c>PlayCard</c>.
    /// <c>null</c> (default) means always playable (subject to zone filter).
    /// </summary>
    public CardDefinitionBuilder WithActivationCondition(KeywordNode condition)
    {
        _activationCondition = condition;
        return this;
    }

    /// <summary>Adds a cost that must be paid before the primary effect executes.</summary>
    public CardDefinitionBuilder AddCost(CostDef cost)
    {
        _costs.Add(cost);
        return this;
    }

    /// <summary>Replaces the entire state map declaration list.</summary>
    public CardDefinitionBuilder WithStateMap(IReadOnlyList<StateFieldDecl> declarations)
    {
        _stateMap = declarations.ToList();
        return this;
    }

    /// <summary>Appends a single named state field to this card's state map.</summary>
    public CardDefinitionBuilder WithStateField(string name, StateFieldType type, string? textTemplate = null)
    {
        _stateMap ??= new List<StateFieldDecl>();
        _stateMap.Add(new StateFieldDecl(name, type, textTemplate));
        return this;
    }

    /// <summary>Builds and returns the <see cref="CardDefinition"/>.</summary>
    public CardDefinition Build() => new(
        Name:                 _name,
        StaticProperties:     _staticProperties,
        PrimaryEffect:        _primaryEffect,
        AdditionalEffects:    _additionalEffects,
        StaticEffects:        _staticEffects,
        ActivationCondition:  _activationCondition,
        Cost:                 _costs.Count > 0 ? _costs : null,
        StateMapDeclarations: _stateMap);
}
