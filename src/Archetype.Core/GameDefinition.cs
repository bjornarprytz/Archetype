namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  Card, Zone, Phase definitions — design-time immutable data (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// Static, design-time definition of a card.  Instantiated into an atom at
/// game setup or when <c>create-card</c> / <c>copy-card</c> is invoked.
/// </summary>
public sealed record CardDefinition(
    string Name,
    IReadOnlyDictionary<string, object> StaticProperties,
    EffectBlockDef PrimaryEffect,
    IReadOnlyList<NamedEffectBlockDef> AdditionalEffects,
    IReadOnlyList<StaticEffectDef> StaticEffects);

/// <summary>
/// A named, activatable effect block on a card (e.g. an activated ability).
/// </summary>
public sealed record NamedEffectBlockDef(
    string Name,
    KeywordNode? ActivationCondition,
    EffectBlockDef? Cost,
    EffectBlockDef Body);

/// <summary>
/// Static, design-time definition of a zone.
/// </summary>
public sealed record ZoneDefinition(
    string Name,
    IReadOnlyDictionary<string, object> StaticProperties);

/// <summary>
/// Definition of a game phase, run in turn order.  <see cref="Init"/> runs
/// at phase start, then the player action window opens, then <see cref="Cleanup"/> runs.
/// </summary>
public sealed record PhaseDefinition(
    string Name,
    EffectBlockDef? Init = null,
    EffectBlockDef? Cleanup = null);

/// <summary>
/// A rule that runs when a named action type occurs (before / after).
/// </summary>
public sealed record ActionRuleDefinition(
    EffectBlockDef? Before = null,
    EffectBlockDef? After = null);

/// <summary>
/// A state-based rule: a condition that, when true, triggers an effect block.
/// Evaluated in registration order until a fixpoint is reached (D7).
/// </summary>
public sealed record StateBasedRule(
    string Name,
    KeywordNode Condition,
    EffectBlockDef Body);

/// <summary>
/// A named grouping of card definitions (for tooling / meta-game).
/// Not used by the engine during execution.
/// </summary>
public sealed record CardSet(string Name, IReadOnlyList<string> Cards);

/// <summary>
/// Static properties that define a player role.  Mutable state (health,
/// resources) belongs in the <see cref="InitManifest"/>.
/// </summary>
public sealed record PlayerDefinition(
    IReadOnlyDictionary<string, object> StaticProperties);

// ---------------------------------------------------------------------------
//  GameDefinition — the immutable aggregate (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// The complete, immutable description of a game.  Produced by
/// <c>GameDefinitionBuilder.Build()</c> or <c>GameDefinitionLoader.FromJson()</c>.
/// Both paths produce an identical runtime object.
/// <para>
/// Built-in keywords are automatically merged into <see cref="Keywords"/> from
/// <see cref="BuiltInKeywords"/> at construction time.  Game-creator keywords
/// may not shadow built-ins.
/// </para>
/// </summary>
public sealed record GameDefinition(
    IReadOnlyDictionary<string, KeywordDefinition> Keywords,
    IReadOnlyDictionary<string, CardDefinition> CardDefinitions,
    IReadOnlyDictionary<string, ZoneDefinition> ZoneDefinitions,
    IReadOnlyDictionary<string, CardSet> CardSets,
    IReadOnlyList<StateBasedRule> StateBasedRules,
    IReadOnlyList<PhaseDefinition> Phases,
    IReadOnlyDictionary<string, IReadOnlyList<ActionRuleDefinition>> ActionRules,
    TriggerResolutionOrder TriggerResolutionOrder,
    IReadOnlyDictionary<string, PlayerDefinition> PlayerDefinitions,
    InitManifest? DefaultInitManifest = null);

/// <summary>Determines the order in which simultaneous triggers resolve (D8).</summary>
public enum TriggerResolutionOrder
{
    /// <summary>Oldest active static effect fires first (default).</summary>
    OldestFirst,
    /// <summary>Newest active static effect fires first.</summary>
    OldestLast,
    /// <summary>The affected player chooses the order.</summary>
    PromptPlayer,
}

// ---------------------------------------------------------------------------
//  InitManifest — declarative desired-state for fresh sessions (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// Declares the initial state of a game session.  The engine provisions
/// this before the first phase: creates zones, then cards, then applies
/// mutable state overrides.  No events are logged during provisioning.
/// </summary>
public sealed record InitManifest(
    IReadOnlyList<ZoneSpec> Zones,
    IReadOnlyList<CardSpec> Cards,
    IReadOnlyList<PlayerStateSpec> PlayerStates);

/// <summary>Specifies a zone to create at session start.</summary>
public sealed record ZoneSpec(
    string LocalId,
    string Owner,
    string Definition,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null);

/// <summary>Specifies a card to create at session start.</summary>
public sealed record CardSpec(
    string Owner,
    string ZoneLocalId,
    string Definition,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null);

/// <summary>Specifies initial mutable state for a player atom.</summary>
public sealed record PlayerStateSpec(
    string Player,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null);

// ---------------------------------------------------------------------------
//  Game result
// ---------------------------------------------------------------------------

/// <summary>
/// The outcome of a completed game session.
/// </summary>
public sealed record GameResult(
    string? Winner,
    IReadOnlyList<GameEvent> FinalLog)
{
    /// <summary>Returns <c>true</c> if the game ended in a draw.</summary>
    public bool IsDraw => Winner is null;
}
