using System.Text.Json.Serialization;

namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  D17 — Save/Load snapshot type hierarchy
//  All types in this file are in Archetype.Core so they can appear in the
//  IEngineObserver.OnTurnStart signature without creating a circular reference.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
//  BoundValue — type-safe serialization wrapper for GameEvent.BoundArgs (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Discriminated union used exclusively at the serialization boundary.
/// <c>GameEvent.BoundArgs</c> is <c>Dictionary&lt;string, object&gt;</c> at
/// runtime; <see cref="BoundValue"/> gives each value type a stable JSON form
/// so round-trips preserve type identity (e.g. <see cref="AtomId"/> does not
/// deserialize as a plain <c>long</c>).
/// </summary>
[JsonDerivedType(typeof(NumberValue),    typeDiscriminator: "num")]
[JsonDerivedType(typeof(BoolValue),      typeDiscriminator: "bool")]
[JsonDerivedType(typeof(StringValue),    typeDiscriminator: "str")]
[JsonDerivedType(typeof(AtomIdValue),    typeDiscriminator: "atom")]
[JsonDerivedType(typeof(ContribIdValue), typeDiscriminator: "contrib")]
[JsonDerivedType(typeof(EventRefValue),  typeDiscriminator: "event")]
[JsonDerivedType(typeof(CollectionValue),typeDiscriminator: "col")]
public abstract record BoundValue;

/// <summary>A <c>double</c> runtime value.</summary>
public sealed record NumberValue(double Value) : BoundValue;

/// <summary>A <c>bool</c> runtime value.</summary>
public sealed record BoolValue(bool Value) : BoundValue;

/// <summary>A <c>string</c> runtime value.</summary>
public sealed record StringValue(string Value) : BoundValue;

/// <summary>An <see cref="AtomId"/> runtime value.</summary>
public sealed record AtomIdValue(long Id) : BoundValue;

/// <summary>A <see cref="ContributionId"/> runtime value.</summary>
public sealed record ContribIdValue(long Id) : BoundValue;

/// <summary>
/// An <see cref="EventRef"/> runtime value — serialized as the referenced
/// event's <see cref="GameEvent.SequenceNumber"/>.  On deserialization the
/// serializer rebuilds the <see cref="EventRef"/> by scanning the snapshot's
/// <c>FinalizedLog</c>.
/// </summary>
public sealed record EventRefValue(long SequenceNumber) : BoundValue;

/// <summary>
/// A collection of runtime values (e.g. the return of <c>get-atoms-in-zone</c>).
/// </summary>
public sealed record CollectionValue(IReadOnlyList<BoundValue> Items) : BoundValue;

// ---------------------------------------------------------------------------
//  RngSnapshot — deterministic RNG state (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Captures the state of <c>SeededRandom</c> at a turn boundary.
/// On load, construct a fresh <c>SeededRandom(Seed)</c> and advance it
/// <see cref="CallCount"/> steps to reproduce the exact next value.
/// </summary>
public sealed record RngSnapshot(long Seed, long CallCount);

// ---------------------------------------------------------------------------
//  StaticEffectDefRef — reference to a declarative effect in GameDefinition
// ---------------------------------------------------------------------------

/// <summary>
/// Identifies a declarative <see cref="StaticEffectDef"/> by its position in
/// <c>CardDefinition.StaticEffects</c>.  Used in
/// <see cref="StaticEffectSnapshot"/> to avoid inlining the full definition
/// for effects that already exist in <see cref="GameDefinition"/>.
/// </summary>
public sealed record StaticEffectDefRef(string CardDefinitionName, int EffectIndex);

// ---------------------------------------------------------------------------
//  DormantEffectSnapshot — dormant declarative effects (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Snapshot of a single dormant declarative effect — an effect whose
/// while-condition is currently false and is waiting to be re-activated.
/// Stored by reference so load can resolve the full
/// <see cref="StaticEffectDef"/> from <see cref="GameDefinition"/>.
/// </summary>
public sealed record DormantEffectSnapshot(
    AtomId OwnerAtomId,
    string CardDefinitionName,
    int    EffectIndex);

// ---------------------------------------------------------------------------
//  AtomSnapshot — per-atom state (snapshot layer) (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Snapshot of a single atom's settled state at a turn boundary.
/// <para>
/// <b>ModifierIndex / ConditionIndex are NOT stored here.</b> They are
/// reconstructed on load by iterating <see cref="GameStateSnapshot.Contributions"/>
/// (Decision 7 in the D17 design doc).
/// </para>
/// <para>
/// <see cref="RefName"/> stores the definition-name string that
/// <c>_atomDefinitionNames</c> maps from this atom's ID (e.g. <c>"test-card"</c>,
/// <c>"hand-zone"</c>).  Null for atoms that have no definition entry (session
/// atom, player atoms).
/// </para>
/// </summary>
public sealed record AtomSnapshotData(
    AtomId  Id,
    AtomKind Kind,
    string?  RefName,
    AtomId   OwnerId,
    AtomId   ZoneId,
    IReadOnlyDictionary<string, double> Accumulators);

// ---------------------------------------------------------------------------
//  ContributionSnapshot — modifier / condition contributions (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Discriminated union for serialized contributions.
/// </summary>
[JsonDerivedType(typeof(ModifierContributionSnapshot),   typeDiscriminator: "mod")]
[JsonDerivedType(typeof(ConditionContributionSnapshot),  typeDiscriminator: "cond")]
public abstract record ContributionSnapshot;

/// <summary>Snapshot of a <see cref="ModifierContribution"/>.</summary>
public sealed record ModifierContributionSnapshot(
    ContributionId       Id,
    ContributionSource   Source,
    AtomId               TargetAtom,
    string               PropertyName,
    ModifierKind         Kind,
    double               Value,
    LifetimeSpec?        Lifetime) : ContributionSnapshot;

/// <summary>Snapshot of a <see cref="ConditionContribution"/>.</summary>
public sealed record ConditionContributionSnapshot(
    ContributionId       Id,
    ContributionSource   Source,
    AtomId               TargetAtom,
    string               ConditionName,
    LifetimeSpec?        Lifetime) : ContributionSnapshot;

// ---------------------------------------------------------------------------
//  StaticEffectSnapshot — active static effect state (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Snapshot of a live <see cref="StaticEffect"/> instance.
/// <para>
/// Exactly one of <see cref="DeclarativeRef"/> and <see cref="DynamicTrigger"/>
/// must be non-null (enforced by the constructor).  Declarative effects are
/// stored by reference and resolved from <see cref="GameDefinition"/> on load;
/// dynamic effects inline their trigger definition.
/// </para>
/// </summary>
public sealed class StaticEffectSnapshot
{
    /// <summary>Stable ID of this static effect instance.</summary>
    public StaticEffectId Id { get; init; }

    /// <summary>Whether this was a declarative or dynamic effect.</summary>
    public bool IsDeclarative { get; init; }

    /// <summary>The atom that owns this effect.</summary>
    public AtomId OwnerAtomId { get; init; }

    /// <summary>The lifetime specification for this effect.</summary>
    public LifetimeSpec Lifetime { get; init; } = LifetimeSpec.Permanent;

    /// <summary>Number of times this effect's trigger has fired.</summary>
    public int TriggerFireCount { get; init; }

    /// <summary>
    /// The trigger high-water mark — the highest sequence number already
    /// scanned by <c>TriggerResolver</c> for this effect.
    /// </summary>
    public long TriggerHighWaterMark { get; init; }

    /// <summary>
    /// Contribution IDs owned by this effect (removed on expiry).
    /// </summary>
    public IReadOnlyList<ContributionId> OwnedContributions { get; init; } =
        Array.Empty<ContributionId>();

    /// <summary>
    /// Non-null for declarative effects: identifies the backing
    /// <see cref="StaticEffectDef"/> in <see cref="GameDefinition"/>.
    /// Mutually exclusive with <see cref="DynamicTrigger"/>.
    /// </summary>
    public StaticEffectDefRef? DeclarativeRef { get; init; }

    /// <summary>
    /// Non-null for dynamic effects: the inlined trigger definition (if any).
    /// Mutually exclusive with <see cref="DeclarativeRef"/>.
    /// </summary>
    public TriggerDefinition? DynamicTrigger { get; init; }

    /// <summary>
    /// Constructs a <see cref="StaticEffectSnapshot"/> and validates that
    /// exactly one of <see cref="DeclarativeRef"/> / <see cref="DynamicTrigger"/>
    /// is non-null.
    /// </summary>
    public StaticEffectSnapshot() { }

    /// <summary>
    /// Validates the snapshot invariant: exactly one of
    /// <see cref="DeclarativeRef"/> or <see cref="DynamicTrigger"/> may be
    /// non-null.  Call after construction when both may be set.
    /// </summary>
    public void ValidateExclusive()
    {
        if (DeclarativeRef is not null && DynamicTrigger is not null)
            throw new InvalidOperationException(
                "StaticEffectSnapshot: DeclarativeRef and DynamicTrigger are mutually exclusive. " +
                "Exactly one must be non-null.");
        // Both null is allowed: a dynamic effect with no trigger (pure state contribution).
    }
}

// ---------------------------------------------------------------------------
//  GameStateSnapshot — the top-level snapshot record (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Complete, settled game state at a turn boundary — sufficient to resume the
/// session from the start of that turn with identical outcomes given the same
/// player responses.
/// <para>
/// Produced by <c>GameState.ToSnapshot()</c> and consumed by
/// <c>GameSessionBuilder.FromSavedState()</c>.  Serialized and deserialized
/// by <c>GameStateSnapshotSerializer</c> in <c>Archetype.Engine</c>.
/// </para>
/// <para>
/// <b>Version:</b> increment when the snapshot schema changes.  Deserialization
/// of an unsupported version should throw rather than silently produce
/// incorrect state.
/// </para>
/// </summary>
public sealed record GameStateSnapshot(
    /// <summary>Schema version — used to detect incompatible snapshots.</summary>
    int Version,

    /// <summary>
    /// The ID of the <see cref="GameDefinition"/> that produced this snapshot.
    /// Validated by <c>GameSessionBuilder.Build()</c> before constructing the
    /// session.
    /// </summary>
    string GameDefinitionId,

    /// <summary>The 1-based turn number at which this snapshot was taken.</summary>
    int TurnNumber,

    /// <summary>Next value from the atom ID counter.</summary>
    long NextAtomId,

    /// <summary>Next value from the contribution / static-effect ID counter.</summary>
    long NextContributionId,

    /// <summary>The ID of the singleton session atom.</summary>
    AtomId SessionAtomId,

    /// <summary>All atoms in settled state.</summary>
    IReadOnlyList<AtomSnapshotData> Atoms,

    /// <summary>
    /// All active modifier and condition contributions.
    /// <c>ModifierIndex</c> and <c>ConditionIndex</c> on each atom are
    /// reconstructed from this list on load.
    /// </summary>
    IReadOnlyList<ContributionSnapshot> Contributions,

    /// <summary>All currently active static effects.</summary>
    IReadOnlyList<StaticEffectSnapshot> ActiveStaticEffects,

    /// <summary>All dormant declarative effects waiting to re-activate.</summary>
    IReadOnlyList<DormantEffectSnapshot> DormantEffects,

    /// <summary>
    /// Player name registry: maps player atom IDs to their string names.
    /// Restored so <c>declare-winner</c> and <c>player-by-name</c> work after
    /// a load.
    /// </summary>
    IReadOnlyDictionary<long, string> PlayerNames,

    /// <summary>
    /// The finalized event log at this turn boundary — all events from
    /// completed turns.  <see cref="EventRefValue"/> entries reference events
    /// in this log by sequence number.
    /// </summary>
    IReadOnlyList<GameEvent> FinalizedLog,

    /// <summary>RNG state for deterministic replay.</summary>
    RngSnapshot Rng)
{
    /// <summary>Current snapshot schema version.</summary>
    public const int CurrentVersion = 1;
}
