using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="TriggerDefinition"/> instances.
/// <example>
/// <code>
/// var trigger = new TriggerDefinitionBuilder("take-damage", TriggerScope.ThisAction)
///     .WithEventParam("target", "victim", TypeName.Card)
///     .WithBinding("target", "damaged-card")
///     .OnFired(b => b.Step("modify-accumulator", Kw.Param("damaged-card"), Kw.Str("poison"), Kw.Num(1)))
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class TriggerDefinitionBuilder
{
    private readonly string _eventKeyword;
    private readonly TriggerScope _scope;
    private KeywordNode? _condition;
    private readonly List<EventParamDecl> _eventParams = new();
    private readonly List<EventBinding> _bindings = new();
    private EffectBlockDef _firedBlock = EffectBlockDef.Empty;

    /// <summary>
    /// Initialises a builder that fires on <paramref name="eventKeyword"/> events
    /// within the given <paramref name="scope"/>.
    /// </summary>
    public TriggerDefinitionBuilder(string eventKeyword, TriggerScope scope)
    {
        _eventKeyword = eventKeyword;
        _scope = scope;
    }

    /// <summary>
    /// Declares an event argument that can be referenced by name in the
    /// trigger condition and fired block.
    /// </summary>
    public TriggerDefinitionBuilder WithEventParam(string argName, string paramName, TypeName type)
    {
        _eventParams.Add(new EventParamDecl(argName, paramName, type));
        return this;
    }

    /// <summary>
    /// Sets a condition that must be true for the trigger to fire.
    /// </summary>
    public TriggerDefinitionBuilder WithCondition(KeywordNode condition)
    {
        _condition = condition;
        return this;
    }

    /// <summary>
    /// Binds an event argument to a named variable in the fired block.
    /// </summary>
    public TriggerDefinitionBuilder WithBinding(string eventArgName, string blockVarName)
    {
        _bindings.Add(new EventBinding(eventArgName, blockVarName));
        return this;
    }

    /// <summary>Sets the effect block that executes when the trigger fires.</summary>
    public TriggerDefinitionBuilder OnFired(EffectBlockDef block)
    {
        _firedBlock = block;
        return this;
    }

    /// <summary>Builds and sets the fired block via an <see cref="EffectBlockBuilder"/> callback.</summary>
    public TriggerDefinitionBuilder OnFired(Action<EffectBlockBuilder> configure)
    {
        var builder = new EffectBlockBuilder();
        configure(builder);
        _firedBlock = builder.Build();
        return this;
    }

    /// <summary>Builds and returns the <see cref="TriggerDefinition"/>.</summary>
    public TriggerDefinition Build() => new(
        EventKeyword:  _eventKeyword,
        Scope:         _scope,
        EventParams:   _eventParams,
        Condition:     _condition,
        EventBindings: _bindings,
        FiredBlock:    _firedBlock);
}
