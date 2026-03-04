using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Runtime snapshot of a single atom.  Mutable — the engine mutates it in
/// place during keyword execution.
/// </summary>
internal sealed class AtomSnapshot
{
    /// <summary>Stable identity assigned at creation; never changes.</summary>
    public AtomId Id { get; init; }

    /// <summary>The kind of this atom.  Immutable after creation.</summary>
    public AtomKind Kind { get; init; }

    /// <summary>
    /// The zone this card currently occupies.  Only meaningful for
    /// <see cref="AtomKind.Card"/> atoms.  Updated by <c>move-card</c>.
    /// </summary>
    public AtomId ZoneId { get; set; } = AtomId.None;

    /// <summary>
    /// The player who owns this card or zone.  Set at creation, immutable thereafter.
    /// Not meaningful for <see cref="AtomKind.Player"/> or <see cref="AtomKind.Session"/>.
    /// </summary>
    public AtomId OwnerId { get; init; } = AtomId.None;

    // -----------------------------------------------------------------------
    //  Mutable state
    // -----------------------------------------------------------------------

    /// <summary>
    /// Named accumulators (e.g. "damage", "health").  Values add permanently
    /// via <c>modify-accumulator</c>; there is no contribution tracking here.
    /// </summary>
    public Dictionary<string, double> Accumulators { get; } = new(StringComparer.Ordinal);

    // Modifier contributions keyed by property name, for O(1) evaluation per property.
    internal Dictionary<string, List<ModifierContribution>> ModifierIndex { get; } =
        new(StringComparer.Ordinal);

    // Condition contributions keyed by condition name; presence = non-empty list.
    internal Dictionary<string, List<ConditionContribution>> ConditionIndex { get; } =
        new(StringComparer.Ordinal);

    // -----------------------------------------------------------------------
    //  Modifier evaluation (D5)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Computes the modifier-adjusted value of a named property:
    /// <c>(base + Σ additives) × Π multiplicatives</c>.
    /// The base value is the current accumulator for that name (0 if absent).
    /// </summary>
    public double GetComputedProperty(string name)
    {
        double @base = Accumulators.TryGetValue(name, out var acc) ? acc : 0.0;

        if (!ModifierIndex.TryGetValue(name, out var mods) || mods.Count == 0)
            return @base;

        double additive       = mods.Where(m => m.Kind == ModifierKind.Additive).Sum(m => m.Value);
        double multiplicative = mods.Where(m => m.Kind == ModifierKind.Multiplicative)
                                    .Aggregate(1.0, (product, m) => product * m.Value);

        return (@base + additive) * multiplicative;
    }

    // -----------------------------------------------------------------------
    //  Condition presence (D5)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the named condition is active (has at least one
    /// active contribution).
    /// </summary>
    public bool HasCondition(string name) =>
        ConditionIndex.TryGetValue(name, out var list) && list.Count > 0;
}

/// <summary>
/// The mutable runtime state of a game session.  Holds all atom snapshots,
/// contribution registries, and active static effect lists.
/// <para>
/// Internal to <c>Archetype.Engine</c>; exposed read-only to tests via
/// <see cref="Core.GameStateView"/> through the
/// <see cref="Core.IGameStateReadable"/> interface.
/// </para>
/// </summary>
internal sealed class GameState : Core.IGameStateReadable
{
    // -----------------------------------------------------------------------
    //  Atom registry
    // -----------------------------------------------------------------------

    private readonly Dictionary<AtomId, AtomSnapshot> _atoms = new();
    private readonly AtomIdCounter _atomIds = new();

    /// <summary>
    /// Allocates a new <see cref="AtomId"/> and registers an
    /// <see cref="AtomSnapshot"/> for it.
    /// </summary>
    public AtomId CreateAtom(AtomKind kind, AtomId ownerId = default, AtomId zoneId = default)
    {
        var id = _atomIds.Next();
        _atoms[id] = new AtomSnapshot { Id = id, Kind = kind, OwnerId = ownerId, ZoneId = zoneId };
        return id;
    }

    /// <summary>
    /// Returns the <see cref="AtomSnapshot"/> for the given atom ID.
    /// Throws <see cref="EngineException"/> if the atom does not exist.
    /// </summary>
    public AtomSnapshot GetAtom(AtomId id)
    {
        if (!_atoms.TryGetValue(id, out var snapshot))
            throw new EngineException($"Atom {id} does not exist in the current game state.");
        return snapshot;
    }

    /// <summary>
    /// Returns <c>true</c> if the atom exists and has the expected kind.
    /// </summary>
    public bool IsAtomOfKind(AtomId id, AtomKind kind) =>
        _atoms.TryGetValue(id, out var s) && s.Kind == kind;

    /// <summary>All atom IDs of the given kind currently in state.</summary>
    public IReadOnlyList<AtomId> GetAtoms(AtomKind kind) =>
        _atoms.Values.Where(a => a.Kind == kind).Select(a => a.Id).ToList();

    // -----------------------------------------------------------------------
    //  Contribution registry (D5)
    // -----------------------------------------------------------------------

    private readonly ContributionIdCounter _contribIds = new();

    /// <summary>Global registry for O(1) removal by ID.</summary>
    internal Dictionary<ContributionId, IContribution> ContributionRegistry { get; } = new();

    /// <summary>All active static effects.</summary>
    internal List<StaticEffect> ActiveStaticEffects { get; } = new();

    /// <summary>Dormant declarative effects waiting for their while-condition to become true.</summary>
    internal List<DormantDeclarativeEffect> DormantDeclarativeEffects { get; } = new();

    /// <summary>Allocates the next contribution ID.</summary>
    public ContributionId NextContributionId() => _contribIds.NextContribution();

    /// <summary>Allocates the next static effect ID.</summary>
    public StaticEffectId NextStaticEffectId() => _contribIds.NextStaticEffect();

    // -----------------------------------------------------------------------
    //  Session atom
    // -----------------------------------------------------------------------

    /// <summary>The singleton session atom; created at provisioning time.</summary>
    public AtomId SessionAtomId { get; private set; } = AtomId.None;

    /// <summary>Creates the session atom and registers it.</summary>
    public AtomId CreateSessionAtom()
    {
        var id = CreateAtom(AtomKind.Session);
        SessionAtomId = id;
        return id;
    }

    // -----------------------------------------------------------------------
    //  IGameStateReadable implementation (for GameStateView)
    // -----------------------------------------------------------------------

    double IGameStateReadable.GetAccumulator(AtomId atom, string name) =>
        GetAtom(atom).Accumulators.TryGetValue(name, out var v) ? v : 0.0;

    bool IGameStateReadable.HasCondition(AtomId atom, string name) =>
        GetAtom(atom).HasCondition(name);

    double IGameStateReadable.GetComputedProperty(AtomId atom, string name) =>
        GetAtom(atom).GetComputedProperty(name);

    AtomId IGameStateReadable.GetZone(AtomId card) => GetAtom(card).ZoneId;

    AtomId IGameStateReadable.GetOwner(AtomId atom) => GetAtom(atom).OwnerId;

    AtomKind IGameStateReadable.GetKind(AtomId atom) => GetAtom(atom).Kind;

    IReadOnlyList<AtomId> IGameStateReadable.GetAtoms(AtomKind kind) => GetAtoms(kind);
}

// ---------------------------------------------------------------------------
//  Contribution interface (internal)
// ---------------------------------------------------------------------------

/// <summary>Common interface for contributions stored in the registry.</summary>
internal interface IContribution
{
    ContributionId Id { get; }
    AtomId TargetAtom { get; }
}

// ---------------------------------------------------------------------------
//  Static effect runtime record (D6, D13)
// ---------------------------------------------------------------------------

/// <summary>
/// A live static effect instance.  Created from a <see cref="StaticEffectDef"/>
/// during card provisioning or when a standing-mutation keyword executes.
/// </summary>
internal sealed class StaticEffect
{
    public StaticEffectId Id { get; init; }
    public AtomId OwnerAtom { get; init; }
    public bool IsDeclarative { get; init; }

    /// <summary>Non-null for declarative effects; used to repopulate the dormant list.</summary>
    public StaticEffectDef? SourceDefinition { get; init; }

    public LifetimeSpec Lifetime { get; init; } = LifetimeSpec.Permanent;

    public int TriggerFireCount { get; set; }
    public long TriggerHighWaterMark { get; set; }

    public ContributionId? StateContribution { get; set; }
    public TriggerDefinition? Trigger { get; init; }
    public ParameterModification? ParameterModification { get; init; }

    public List<ContributionId> OwnedContributions { get; } = new();
}

/// <summary>
/// A dormant declarative effect: the while-condition is currently false, so
/// no <see cref="StaticEffect"/> instance is active.  Will be re-instantiated
/// by Phase 2 of <c>CheckLifetimes</c> when the condition becomes true.
/// </summary>
internal sealed class DormantDeclarativeEffect
{
    public AtomId OwnerAtom { get; init; }
    public StaticEffectDef EffectDef { get; init; } = null!;
}
