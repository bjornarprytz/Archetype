namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  Player interaction interfaces (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// The unified per-player interface through which all player interactions
/// flow.  The engine calls this interface; the host (Godot or test harness)
/// implements it.  Every call site that previously referenced
/// <c>IPromptChannel</c> now goes through this interface (D14/A15).
/// </summary>
public interface IPlayerStrategy
{
    /// <summary>
    /// Called when the engine opens an action window for this player.
    /// Return <c>null</c> to pass (close the action window).
    /// </summary>
    Task<PlayerAction?> SelectActionAsync(
        AvailableActions available,
        GameStateView state,
        CancellationToken ct = default);

    /// <summary>
    /// Called for all engine-initiated prompts: mid-effect target or value
    /// selection, and trigger-order selection in <c>PromptPlayer</c> mode.
    /// </summary>
    Task<PromptResponse> RespondToPromptAsync(
        PromptContext context,
        GameStateView state,
        CancellationToken ct = default);
}

// ---------------------------------------------------------------------------
//  IEngineObserver (D7)
// ---------------------------------------------------------------------------

/// <summary>
/// Optional host interface for observing engine lifecycle events.
/// <para>
/// <b>OnTurnStart (D17):</b> called at the start of each turn with a complete
/// <see cref="GameStateSnapshot"/>, giving the host the opportunity to persist
/// the snapshot.  Called before any phase init block for that turn executes,
/// so the snapshot captures fully settled end-of-prior-turn state.
/// </para>
/// <para>
/// <b>OnTriggerCascadeAsync (D7):</b> called before each trigger-resolution
/// batch.  Returning <see cref="CascadeDirective.Halt"/> exits the cascade
/// loop cleanly.
/// </para>
/// <para>
/// <b>OnDiagnostic (D25):</b> called by the engine when an <c>assert</c>
/// built-in fails with <c>notify: on</c>.  Called BEFORE raising
/// <see cref="EngineException"/> when <c>on_fail: panic</c>.  Must not throw;
/// any exception is treated as an engine error.  Does NOT write to the event
/// log.
/// </para>
/// </summary>
public interface IEngineObserver
{
    /// <summary>
    /// Called at the start of each turn before any phase init block executes.
    /// The snapshot captures fully settled state (all end-of-prior-turn processing
    /// complete).  The host may persist the snapshot for save/load purposes.
    /// </summary>
    /// <param name="turnNumber">The 1-based turn number about to begin.</param>
    /// <param name="snapshot">The settled game state at this turn boundary.</param>
    Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot);

    /// <summary>
    /// Called before each trigger-resolution batch.
    /// </summary>
    /// <param name="iterationCount">
    /// Number of trigger-resolution batches completed so far in this action
    /// (1-based on the first call).
    /// </param>
    Task<CascadeDirective> OnTriggerCascadeAsync(int iterationCount);

    /// <summary>
    /// Called when an <c>assert</c> built-in fails with <c>notify: on</c>.
    /// <para>
    /// When <c>on_fail: panic</c>, this is called BEFORE the
    /// <see cref="EngineException"/> is raised.  This method is sync void;
    /// it must not throw (the caller treats any exception as an engine error).
    /// </para>
    /// <para>
    /// This method is NOT called when <c>notify: off</c> or when inside a
    /// cost body (<c>IsCostBody = true</c>).
    /// </para>
    /// </summary>
    /// <param name="e">The diagnostic event describing the assertion failure.</param>
    void OnDiagnostic(DiagnosticEvent e);
}

/// <summary>Directive returned by <see cref="IEngineObserver"/>.</summary>
public enum CascadeDirective
{
    /// <summary>Continue the cascade loop normally.</summary>
    Continue,
    /// <summary>
    /// Halt the cascade loop cleanly.  No state is rolled back; the action
    /// is otherwise complete.
    /// </summary>
    Halt,
}

// ---------------------------------------------------------------------------
//  IRandomSource (D9)
// ---------------------------------------------------------------------------

/// <summary>
/// Abstraction over randomness.  Injected at session construction time.
/// The engine provides <c>SeededRandom</c> as the default implementation;
/// tests inject a <c>MockRandomSource</c>.
/// </summary>
public interface IRandomSource
{
    /// <summary>Returns a uniformly random integer in <c>[minInclusive, maxInclusive]</c>.</summary>
    int NextInt(int minInclusive, int maxInclusive);

    /// <summary>Shuffles <paramref name="list"/> in place using Fisher-Yates.</summary>
    void Shuffle<T>(IList<T> list);
}

// ---------------------------------------------------------------------------
//  Player actions and prompts (D3, D8, D14)
// ---------------------------------------------------------------------------

/// <summary>A player action submitted through <see cref="IPlayerStrategy"/>.</summary>
public abstract record PlayerAction;

/// <summary>
/// The player plays a card from hand.
/// <para>
/// <b>CostChoices (D21):</b> player-provided arguments that bind into
/// <see cref="CostDef.Parameters"/> for each cost on this card.  May be empty
/// if no cost declares parameters.
/// </para>
/// <para>
/// <b>Targets (D19 deferred):</b> target selections; currently always empty.
/// </para>
/// </summary>
public sealed record PlayCard(
    AtomId Card,
    IReadOnlyDictionary<string, object>? CostChoices = null,
    IReadOnlyList<AtomId>? Targets = null) : PlayerAction;

/// <summary>
/// The player activates a named ability on an atom.
/// <para>
/// <b>CostChoices (D21):</b> player-provided arguments that bind into
/// <see cref="CostDef.Parameters"/> for each cost on this ability.  May be empty
/// if no cost declares parameters.
/// </para>
/// <para>
/// <b>Targets (D19 deferred):</b> target selections; currently always empty.
/// </para>
/// </summary>
public sealed record ActivateAbility(
    AtomId Source,
    string EffectName,
    IReadOnlyDictionary<string, object>? CostChoices = null,
    IReadOnlyList<AtomId>? Targets = null) : PlayerAction;

/// <summary>The player passes (ends their action window).</summary>
public sealed record Pass : PlayerAction;

/// <summary>
/// The set of actions available to the active player at a given decision point.
/// Computed by the engine before calling <see cref="IPlayerStrategy.SelectActionAsync"/>.
/// <para>
/// <b>ValidateActionArgs (D22):</b> a delegate that validates cost affordability
/// for a candidate action.  The delegate is constructed at
/// <c>ComputeAvailableActions</c> time; it captures a snapshot of the current
/// <see cref="GameState"/> and is safe to call multiple times before returning
/// an action from <see cref="IPlayerStrategy.SelectActionAsync"/>.
/// </para>
/// </summary>
public sealed record AvailableActions(
    IReadOnlyList<PlayableCardOption> PlayableCards,
    IReadOnlyList<ActivatableAbilityOption> ActivatableAbilities,
    bool CanPass,
    Func<PlayerAction, ValidationResult> ValidateActionArgs);

/// <summary>A card option presented to the player.</summary>
public sealed record PlayableCardOption(AtomId Card, IReadOnlyList<TargetSet> ValidTargets);

/// <summary>An ability activation option presented to the player.</summary>
public sealed record ActivatableAbilityOption(
    AtomId Source,
    string EffectName,
    IReadOnlyList<TargetSet> ValidTargets);

/// <summary>A pre-validated set of targets for a single targeting prompt.</summary>
public sealed record TargetSet(IReadOnlyList<AtomId> Targets);

/// <summary>Context for a prompt sent to a player during effect execution (D3, D8).</summary>
public abstract record PromptContext;

/// <summary>A mid-effect prompt asking the player to choose among presented options.</summary>
public sealed record ChoicePrompt(
    string VariableName,
    IReadOnlyList<object> Options,
    int MinChoices,
    int MaxChoices) : PromptContext;

/// <summary>
/// A prompt asking the player to sequence simultaneous trigger groups.
/// Used in <see cref="TriggerResolutionOrder.PromptPlayer"/> mode.
/// </summary>
public sealed record TriggerOrderPrompt(
    IReadOnlyList<TriggerGroup> Groups) : PromptContext;

/// <summary>A group of simultaneous triggers the player may reorder.</summary>
public sealed record TriggerGroup(
    StaticEffectId EffectId,
    string Description);

/// <summary>The player's response to a <see cref="PromptContext"/>.</summary>
public abstract record PromptResponse;

/// <summary>Response to a <see cref="ChoicePrompt"/>.</summary>
public sealed record ChoiceResponse(IReadOnlyList<object> Chosen) : PromptResponse;

/// <summary>Response to a <see cref="TriggerOrderPrompt"/>.</summary>
public sealed record TriggerOrderResponse(IReadOnlyList<StaticEffectId> Order) : PromptResponse;

// ---------------------------------------------------------------------------
//  GameStateView — read-only projection of runtime state (D15)
// ---------------------------------------------------------------------------

/// <summary>
/// A read-only view of the current game state passed to
/// <see cref="IPlayerStrategy"/> methods.  A thin wrapper — no deep copy.
/// </summary>
/// <remarks>
/// Designed to be GDScript-friendly (D15): plain C# types, no heavy generics
/// leaking into the projection.
/// </remarks>
public sealed class GameStateView
{
    private readonly IGameStateReadable _inner;
    private readonly IReadOnlyDictionary<AtomId, string>? _definitionNames;
    private readonly GameDefinition? _definition;
    private readonly IReadOnlyList<string>? _phaseNames;

    private IReadOnlyList<GameEvent> _lastActionEvents = Array.Empty<GameEvent>();

    /// <summary>Initialises a view backed by <paramref name="inner"/>.</summary>
    public GameStateView(IGameStateReadable inner) => _inner = inner;

    /// <summary>
    /// Initialises a view with definition-name and static-property lookup support.
    /// </summary>
    public GameStateView(
        IGameStateReadable inner,
        IReadOnlyDictionary<AtomId, string> definitionNames,
        GameDefinition definition)
    {
        _inner = inner;
        _definitionNames = definitionNames;
        _definition = definition;
        _phaseNames = definition.Phases.Select(p => p.Name).ToList();
    }

    /// <summary>
    /// The events appended during the most recently completed
    /// <c>ResolveAction</c> call.  Reset to an empty list at the start of
    /// each new action and populated after all blocks, SBRs, and triggers
    /// have resolved (D30).
    /// <para>
    /// Used by the Godot interop wrapper to emit GDScript signals after each
    /// action without requiring changes to <see cref="IEngineObserver"/> or
    /// the engine's execution path.
    /// </para>
    /// </summary>
    public IReadOnlyList<GameEvent> LastActionEvents => _lastActionEvents;

    /// <summary>
    /// Sets the <see cref="LastActionEvents"/> list.  Called by the engine
    /// after <c>ResolveAction</c> completes.  Not part of the game-creator API.
    /// </summary>
    public void SetLastActionEvents(IReadOnlyList<GameEvent> events) =>
        _lastActionEvents = events;

    /// <summary>Returns the accumulated value for the named property on an atom.</summary>
    public double GetAccumulator(AtomId atom, string name) =>
        _inner.GetAccumulator(atom, name);

    /// <summary>Returns <c>true</c> if the named condition is present on the atom.</summary>
    public bool HasCondition(AtomId atom, string name) =>
        _inner.HasCondition(atom, name);

    /// <summary>Returns the computed (modifier-adjusted) value of a property on an atom.</summary>
    public double GetComputedProperty(AtomId atom, string name) =>
        _inner.GetComputedProperty(atom, name);

    /// <summary>Returns the zone the card currently occupies.</summary>
    public AtomId GetZone(AtomId card) => _inner.GetZone(card);

    /// <summary>Returns the owner of an atom.</summary>
    public AtomId GetOwner(AtomId atom) => _inner.GetOwner(atom);

    /// <summary>Returns the <see cref="AtomKind"/> of an atom.</summary>
    public AtomKind GetKind(AtomId atom) => _inner.GetKind(atom);

    /// <summary>Returns all atom IDs of the given kind present in the current state.</summary>
    public IReadOnlyList<AtomId> GetAtoms(AtomKind kind) => _inner.GetAtoms(kind);

    /// <summary>Returns a snapshot of all accumulators currently set on the atom.</summary>
    public IReadOnlyDictionary<string, double> GetAccumulators(AtomId atom) => _inner.GetAccumulators(atom);

    /// <summary>Returns the names of all conditions currently active on the atom.</summary>
    public IReadOnlyList<string> GetActiveConditions(AtomId atom) => _inner.GetActiveConditions(atom);

    /// <summary>Returns the property names that have at least one active modifier on the atom.</summary>
    public IReadOnlyList<string> GetModifierKeys(AtomId atom) => _inner.GetModifierKeys(atom);

    /// <summary>Returns the name of the current phase, derived from the phase index accumulator.</summary>
    public string GetCurrentPhaseName()
    {
        if (_phaseNames is null || _phaseNames.Count == 0) return string.Empty;
        var sessionAtoms = _inner.GetAtoms(AtomKind.Session);
        if (sessionAtoms.Count == 0) return string.Empty;
        var idx = (int)_inner.GetAccumulators(sessionAtoms[0]).GetValueOrDefault("phase-index");
        return idx >= 0 && idx < _phaseNames.Count ? _phaseNames[idx] : string.Empty;
    }

    /// <summary>Returns the names of all registered card definitions.</summary>
    public IReadOnlyList<string> GetCardDefinitionNames() =>
        _definition?.CardDefinitions.Keys.ToArray() ?? Array.Empty<string>();

    /// <summary>Returns the names of all registered zone definitions.</summary>
    public IReadOnlyList<string> GetZoneDefinitionNames() =>
        _definition?.ZoneDefinitions.Keys.ToArray() ?? Array.Empty<string>();

    /// <summary>Returns the names of all registered keywords (built-in and game-creator).</summary>
    public IReadOnlyList<string> GetKeywordNames() =>
        _definition?.Keywords.Keys.ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Returns the static property keys defined on this atom's definition,
    /// or an empty list if the atom has no definition.
    /// </summary>
    public IReadOnlyList<string> GetStaticPropertyKeys(AtomId atom)
    {
        if (_definition is null || _definitionNames is null) return Array.Empty<string>();
        if (!_definitionNames.TryGetValue(atom, out var defName)) return Array.Empty<string>();
        if (_definition.CardDefinitions.TryGetValue(defName, out var card)) return card.StaticProperties.Keys.ToArray();
        if (_definition.ZoneDefinitions.TryGetValue(defName, out var zone)) return zone.StaticProperties.Keys.ToArray();
        if (_definition.PlayerDefinitions.TryGetValue(defName, out var player)) return player.StaticProperties.Keys.ToArray();
        return Array.Empty<string>();
    }

    /// <summary>
    /// Returns the definition name the atom was instantiated from, or an empty
    /// string if unknown (e.g. the session atom).
    /// </summary>
    public string GetDefinitionName(AtomId atom) =>
        _definitionNames?.TryGetValue(atom, out var name) == true ? name : string.Empty;

    /// <summary>
    /// Returns the value of a static property on the atom's definition, or
    /// <c>null</c> if the atom has no definition or the key is absent.
    /// </summary>
    public object? GetStaticProperty(AtomId atom, string key)
    {
        if (_definition is null || _definitionNames is null) return null;
        if (!_definitionNames.TryGetValue(atom, out var defName)) return null;
        if (_definition.CardDefinitions.TryGetValue(defName, out var card) &&
            card.StaticProperties.TryGetValue(key, out var cv)) return cv;
        if (_definition.ZoneDefinitions.TryGetValue(defName, out var zone) &&
            zone.StaticProperties.TryGetValue(key, out var zv)) return zv;
        if (_definition.PlayerDefinitions.TryGetValue(defName, out var player) &&
            player.StaticProperties.TryGetValue(key, out var pv)) return pv;
        return null;
    }
}

/// <summary>
/// Interface implemented by <c>GameState</c> (in <c>Archetype.Engine</c>) to
/// serve <see cref="GameStateView"/> without exposing mutable internals.
/// Public so the engine can implement it cross-assembly; it is not part of
/// the game-creator API surface.
/// </summary>
public interface IGameStateReadable
{
    /// <summary>Returns the current value of the named accumulator on <paramref name="atom"/>, or 0 if absent.</summary>
    double GetAccumulator(AtomId atom, string name);

    /// <summary>Returns <c>true</c> if the named condition is active on <paramref name="atom"/>.</summary>
    bool HasCondition(AtomId atom, string name);

    /// <summary>Returns the modifier-adjusted value of the named property on <paramref name="atom"/>.</summary>
    double GetComputedProperty(AtomId atom, string name);

    /// <summary>Returns the zone the card currently occupies (<see cref="AtomId.None"/> if unzoned).</summary>
    AtomId GetZone(AtomId card);

    /// <summary>Returns the owner atom of <paramref name="atom"/> (<see cref="AtomId.None"/> if unowned).</summary>
    AtomId GetOwner(AtomId atom);

    /// <summary>Returns the <see cref="AtomKind"/> of <paramref name="atom"/>.</summary>
    AtomKind GetKind(AtomId atom);

    /// <summary>Returns all atom IDs of the given <paramref name="kind"/> currently in state.</summary>
    IReadOnlyList<AtomId> GetAtoms(AtomKind kind);

    /// <summary>Returns a snapshot of all accumulators currently set on <paramref name="atom"/>.</summary>
    IReadOnlyDictionary<string, double> GetAccumulators(AtomId atom);

    /// <summary>Returns the names of all conditions currently active on <paramref name="atom"/>.</summary>
    IReadOnlyList<string> GetActiveConditions(AtomId atom);

    /// <summary>Returns the property names that have at least one active modifier on <paramref name="atom"/>.</summary>
    IReadOnlyList<string> GetModifierKeys(AtomId atom);
}
