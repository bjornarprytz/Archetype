using System.Text.Json.Serialization;

namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  Static effect definitions (design-time data)
// ---------------------------------------------------------------------------

/// <summary>
/// Design-time definition of a static effect.  Stored in <see cref="CardDefinition"/>
/// and used to instantiate <c>StaticEffect</c> runtime instances on card creation.
/// </summary>
public sealed record StaticEffectDef(
    LifetimeSpec Lifetime,
    EffectBlockDef? StateContributionBlock = null,
    TriggerDefinition? Trigger = null,
    ParameterModification? ParameterModification = null);

/// <summary>
/// Defines when a trigger fires and what block to execute.
/// </summary>
public sealed record TriggerDefinition(
    string EventKeyword,
    TriggerScope Scope,
    IReadOnlyList<EventParamDecl> EventParams,
    KeywordNode? Condition,
    IReadOnlyList<EventBinding> EventBindings,
    EffectBlockDef FiredBlock);

/// <summary>How far back the trigger's condition can see into the event log.</summary>
public enum TriggerScope
{
    /// <summary>Only events from the current action are visible.</summary>
    ThisAction,
    /// <summary>Events from the current turn are visible.</summary>
    ThisTurn,
    /// <summary>All events in the game are visible.</summary>
    ThisGame,
}

/// <summary>
/// Maps a key in <see cref="GameEvent.BoundArgs"/> to a parameter name the
/// trigger condition can reference by <see cref="ParameterRef"/>.
/// </summary>
public sealed record EventParamDecl(string ArgName, string ParamName, TypeName Type);

/// <summary>
/// Pre-populates a friendly variable in the triggered block's bindings from
/// a key in the triggering event's <see cref="GameEvent.BoundArgs"/>.
/// </summary>
public sealed record EventBinding(string EventArgName, string BlockVarName);

// ---------------------------------------------------------------------------
//  Parameter modification definitions (D13)
// ---------------------------------------------------------------------------

/// <summary>
/// Intercepts a mutation keyword invocation either to adjust argument values
/// or to cancel the invocation entirely.
/// </summary>
[JsonDerivedType(typeof(ParameterAdjustment), typeDiscriminator: "adj")]
[JsonDerivedType(typeof(Disable),             typeDiscriminator: "dis")]
public abstract record ParameterModification(
    string TargetKeyword,
    IReadOnlyList<EventParamDecl>? ArgFilter,
    KeywordNode? FilterCondition);

/// <summary>Adjusts the arguments of a mutation keyword invocation.</summary>
public sealed record ParameterAdjustment(
    string TargetKeyword,
    IReadOnlyList<EventParamDecl>? ArgFilter,
    KeywordNode? FilterCondition,
    IReadOnlyList<ParamMod> ParamMods)
    : ParameterModification(TargetKeyword, ArgFilter, FilterCondition);

/// <summary>
/// Cancels a mutation keyword invocation entirely.  Produces a
/// <c>keyword-disabled</c> engine event instead of the normal event.
/// </summary>
public sealed record Disable(
    string TargetKeyword,
    IReadOnlyList<EventParamDecl>? ArgFilter,
    KeywordNode? FilterCondition)
    : ParameterModification(TargetKeyword, ArgFilter, FilterCondition);

/// <summary>A single modification to one argument of an intercepted keyword.</summary>
public sealed record ParamMod(string ParamName, ParamModKind Kind, KeywordNode Expression);

/// <summary>The stacking mode for a parameter modification.</summary>
public enum ParamModKind
{
    /// <summary>Adds a delta to the argument.</summary>
    Additive,
    /// <summary>Multiplies the argument by a factor.</summary>
    Multiplicative,
    /// <summary>Replaces the argument value outright.</summary>
    Replace,
}
