namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  Card, Zone, Phase definitions — design-time immutable data (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// Static, design-time definition of a card.  Instantiated into an atom at
/// game setup or when <c>create-card</c> / <c>copy-card</c> is invoked.
/// <para>
/// <b>ActivationCondition (D19):</b> optional guard evaluated in pure mode
/// before offering <c>PlayCard</c> for this card.  <c>null</c> means always
/// playable (subject to zone filter).  The card atom is pre-bound as
/// <c>source</c> (D13) — <c>ComputeAvailableActions</c> must inject this
/// binding explicitly; the <c>WhileCondition</c> path has a <c>StaticEffect</c>
/// to do so, but the activation-condition path does not.
/// </para>
/// <para>
/// <b>Cost (D20):</b> an ordered list of cost definitions that must be paid
/// before the <see cref="PrimaryEffect"/> executes.  Empty = no cost.
/// Each <see cref="CostDef.Body"/> runs in declaration order within the same
/// action scope as the primary effect (D23).
/// </para>
/// </summary>
public sealed record CardDefinition(
    string Name,
    IReadOnlyDictionary<string, object> StaticProperties,
    EffectBlockDef PrimaryEffect,
    IReadOnlyList<NamedEffectBlockDef> AdditionalEffects,
    IReadOnlyList<StaticEffectDef> StaticEffects,
    KeywordNode? ActivationCondition = null,
    IReadOnlyList<CostDef>? Cost = null,
    /// <summary>
    /// Declared mutable state fields for instances of this card definition.
    /// <c>null</c> is treated as empty — existing callers are unaffected.
    /// </summary>
    IReadOnlyList<StateFieldDecl>? StateMapDeclarations = null);

/// <summary>
/// A named, activatable effect block on a card (e.g. an activated ability).
/// <para>
/// <b>Cost (D20):</b> an ordered list of cost definitions.  Empty = no cost.
/// Replaces the prior <c>Cost: EffectBlockDef?</c> field (D25 breaking change).
/// Each <see cref="CostDef.Body"/> runs in declaration order within the same
/// action scope as <see cref="Body"/> (D23).
/// </para>
/// </summary>
public sealed record NamedEffectBlockDef(
    string Name,
    KeywordNode? ActivationCondition,
    IReadOnlyList<CostDef>? Cost,
    EffectBlockDef Body);

/// <summary>
/// Static, design-time definition of a zone.
/// </summary>
public sealed record ZoneDefinition(
    string Name,
    IReadOnlyDictionary<string, object> StaticProperties,
    /// <summary>
    /// Declared mutable state fields for instances of this zone definition.
    /// <c>null</c> is treated as empty — existing callers are unaffected.
    /// </summary>
    IReadOnlyList<StateFieldDecl>? StateMapDeclarations = null);

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
/// A distributable package of card definitions (D32).
/// <para>
/// Card sets are authored in C# and serialized to JSON by <c>BuildRunner</c>.
/// At runtime the Godot host loads JSON card set files from disk and merges
/// them into the <see cref="GameDefinition"/> via
/// <see cref="GameDefinition.WithCardSets"/>.
/// </para>
/// </summary>
public sealed record CardSet(
    string Name,
    int FormatVersion,
    IReadOnlyList<CardDefinition> Cards);

/// <summary>
/// Static properties that define a player role.  Mutable state (health,
/// resources) belongs in the <see cref="InitManifest"/>.
/// </summary>
public sealed record PlayerDefinition(
    IReadOnlyDictionary<string, object> StaticProperties,
    /// <summary>
    /// Declared mutable state fields for instances of this player definition.
    /// <c>null</c> is treated as empty — existing callers are unaffected.
    /// </summary>
    IReadOnlyList<StateFieldDecl>? StateMapDeclarations = null);

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
/// <para>
/// <b>Id (D17):</b> a non-empty string that uniquely identifies this game definition.
/// Required by <c>GameSessionBuilder.Build()</c>.  Stored in
/// <see cref="GameStateSnapshot"/> so the load path can validate that the snapshot
/// was produced by the same game definition.
/// </para>
/// <para>
/// <b>InitManifest (D29):</b> required; non-nullable.  Declares the initial state
/// of a game session (zones, cards, player state).
/// <c>GameDefinitionBuilder.Build()</c> throws <see cref="DefinitionException"/>
/// if this is not set.  An empty manifest (all empty lists) is valid.
/// </para>
/// <para>
/// <b>PlayableZoneNames (D19):</b> zone definition names from which cards may
/// be played.  <c>null</c> = no zone filter.  Specify one or more names
/// (e.g. <c>"hand"</c>) to enforce zone restrictions on <c>PlayCard</c>.
/// </para>
/// </summary>
public sealed record GameDefinition(
    IReadOnlyDictionary<string, KeywordDefinition> Keywords,
    IReadOnlyDictionary<string, CardDefinition> CardDefinitions,
    IReadOnlyDictionary<string, ZoneDefinition> ZoneDefinitions,
    IReadOnlyList<StateBasedRule> StateBasedRules,
    IReadOnlyList<PhaseDefinition> Phases,
    IReadOnlyDictionary<string, IReadOnlyList<ActionRuleDefinition>> ActionRules,
    TriggerResolutionOrder TriggerResolutionOrder,
    IReadOnlyDictionary<string, PlayerDefinition> PlayerDefinitions,
    // D29: InitManifest is now required (non-nullable, renamed from DefaultInitManifest).
    // GameDefinitionBuilder.Build() throws DefinitionException if not set.
    InitManifest InitManifest,
    IReadOnlyList<string>? PlayableZoneNames = null,
    string? Id = null,
    /// <summary>
    /// Additional state fields declared on the singleton session atom by the game creator.
    /// The two engine-reserved session fields (<c>turn-number</c> and <c>phase-index</c>)
    /// are implicitly declared and must not appear here.
    /// <c>null</c> is treated as empty — existing callers are unaffected.
    /// </summary>
    IReadOnlyList<StateFieldDecl>? SessionStateMapDeclarations = null,
    /// <summary>
    /// Optional build-time AtomGroups defining cross-cutting transformations.
    /// Stored in exported GameDefinition JSON so hosts or tooling can inspect them.
    /// </summary>
    IReadOnlyList<AtomGroupDef>? AtomGroups = null)
{
    /// <summary>
    /// Returns a new <see cref="GameDefinition"/> with the card definitions
    /// from each <see cref="CardSet"/> merged in additively (D32).
    /// <para>
    /// Keyword references in card effect blocks are validated against
    /// <see cref="Keywords"/> at merge time; unknown references throw
    /// <see cref="DefinitionException"/>.
    /// </para>
    /// </summary>
    /// <exception cref="DefinitionException">
    /// Thrown when a card name appears in more than one set (or already exists
    /// in <see cref="CardDefinitions"/>), or when a card's effect block
    /// references an unknown keyword name.
    /// </exception>
    public GameDefinition WithCardSets(IEnumerable<CardSet> sets)
    {
        var merged = new Dictionary<string, CardDefinition>(CardDefinitions);

        foreach (var set in sets)
        {
            foreach (var card in set.Cards)
            {
                if (merged.ContainsKey(card.Name))
                    throw new DefinitionException(
                        $"Duplicate card name '{card.Name}' found while merging card set '{set.Name}'.");

                ValidateCardKeywordRefs(card, Keywords.Keys.ToHashSet(), set.Name);
                merged[card.Name] = card;
            }
        }

        return this with { CardDefinitions = merged };
    }

    private static void ValidateCardKeywordRefs(
        CardDefinition card,
        HashSet<string> knownKeywords,
        string setName)
    {
        ValidateBlock(card.PrimaryEffect, card.Name, setName, knownKeywords);
        foreach (var extra in card.AdditionalEffects)
            ValidateBlock(extra.Body, card.Name, setName, knownKeywords);
        foreach (var cost in card.Cost ?? [])
            ValidateBlock(cost.Body, card.Name, setName, knownKeywords);
    }

    private static void ValidateBlock(
        EffectBlockDef block,
        string cardName,
        string setName,
        HashSet<string> knownKeywords)
    {
        foreach (var step in block.Steps)
        {
            if (!knownKeywords.Contains(step.KeywordName))
                throw new DefinitionException(
                    $"Card '{cardName}' in set '{setName}' references unknown keyword '{step.KeywordName}'.");
            foreach (var arg in step.ArgNodes)
                ValidateNode(arg, knownKeywords, cardName, setName);
        }
    }

    private static void ValidateNode(
        KeywordNode node,
        HashSet<string> knownKeywords,
        string cardName,
        string setName)
    {
        if (node is Invocation inv)
        {
            if (!knownKeywords.Contains(inv.KeywordName))
                throw new DefinitionException(
                    $"Card '{cardName}' in set '{setName}' references unknown keyword '{inv.KeywordName}'.");
            foreach (var arg in inv.Args)
                ValidateNode(arg, knownKeywords, cardName, setName);
        }
    }
}

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
/// <para>
/// <b>D29:</b> Required on every <see cref="GameDefinition"/>.
/// <c>GameDefinitionBuilder.Build()</c> throws <see cref="DefinitionException"/>
/// if not set.  Use <see cref="Empty"/> for games that build all state through
/// phase init blocks.
/// </para>
/// </summary>
public sealed record InitManifest(
    IReadOnlyList<ZoneSpec> Zones,
    IReadOnlyList<CardSpec> Cards,
    IReadOnlyList<PlayerStateSpec> PlayerStates)
{
    /// <summary>
    /// An empty <see cref="InitManifest"/> with no zones, cards, or player states.
    /// Valid for games that build all state through phase init blocks.
    /// </summary>
    public static readonly InitManifest Empty = new(
        Array.Empty<ZoneSpec>(),
        Array.Empty<CardSpec>(),
        Array.Empty<PlayerStateSpec>());
}

/// <summary>Specifies a zone to create at session start.</summary>
public sealed record ZoneSpec(
    string LocalId,
    string Owner,
    string Definition,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null);

/// <summary>
/// Specifies a card to create at session start.
/// <para>
/// <b>LocalId (D29):</b> optional.  When set, uniquely identifies this card
/// instance within its manifest so that <see cref="AtomStateOverride"/> or
/// <see cref="HostManifest"/> can reference it by name.  Must be unique within
/// <see cref="InitManifest.Cards"/> (non-null values only).  Card LocalIds and
/// zone LocalIds do not share a namespace.
/// </para>
/// </summary>
public sealed record CardSpec(
    string Owner,
    string ZoneLocalId,
    string Definition,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null,
    // D29: optional; required only when host state overrides target this card.
    string? LocalId = null);

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
