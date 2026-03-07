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

/// <summary>The player plays a card from hand.</summary>
public sealed record PlayCard(AtomId Card) : PlayerAction;

/// <summary>The player activates a named ability on an atom.</summary>
public sealed record ActivateAbility(AtomId Source, string EffectName) : PlayerAction;

/// <summary>The player passes (ends their action window).</summary>
public sealed record Pass : PlayerAction;

/// <summary>
/// The set of actions available to the active player at a given decision point.
/// Computed by the engine before calling <see cref="IPlayerStrategy.SelectActionAsync"/>.
/// </summary>
public sealed record AvailableActions(
    IReadOnlyList<PlayableCardOption> PlayableCards,
    IReadOnlyList<ActivatableAbilityOption> ActivatableAbilities,
    bool CanPass);

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

    /// <summary>
    /// Wraps an <see cref="IGameStateReadable"/> in a read-only view.
    /// Callers outside the engine assembly obtain this through
    /// <see cref="IPlayerStrategy"/> callbacks; the engine constructs it here.
    /// </summary>
    public GameStateView(IGameStateReadable inner) => _inner = inner;

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
}
