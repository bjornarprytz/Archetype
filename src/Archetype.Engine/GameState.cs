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

    /// <summary>
    /// All atom IDs in state, regardless of kind.
    /// Used by <c>get-atoms-in-zone</c> to enumerate every atom whose zone matches.
    /// </summary>
    public IReadOnlyList<AtomId> GetAllAtoms() =>
        _atoms.Keys.ToList();

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
    //  Player name registry (populated by manifest provisioning)
    // -----------------------------------------------------------------------

    private readonly Dictionary<AtomId, string> _playerNames    = new();
    private readonly Dictionary<string, AtomId> _playerAtomsByName = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a player name keyed by their atom ID.  Called during manifest
    /// provisioning so that <c>declare-winner</c> can resolve a player atom to
    /// the string name stored in <see cref="GameResult"/>, and
    /// <c>player-by-name</c> can reverse-look-up an atom from a name string.
    /// </summary>
    public void RegisterPlayerName(AtomId id, string name)
    {
        _playerNames[id]       = name;
        _playerAtomsByName[name] = id;
    }

    /// <summary>
    /// Attempts to resolve a player atom ID to its registered name.
    /// Returns <c>false</c> if the atom was not registered as a player.
    /// </summary>
    public bool TryGetPlayerName(AtomId id, out string name) =>
        _playerNames.TryGetValue(id, out name!);

    /// <summary>
    /// Attempts to resolve a player name string to its atom ID.
    /// Returns <c>false</c> if no player with that name was registered.
    /// Used by the <c>player-by-name</c> primitive.
    /// </summary>
    public bool TryGetPlayerAtomByName(string name, out AtomId id) =>
        _playerAtomsByName.TryGetValue(name, out id);

    // -----------------------------------------------------------------------
    //  Game outcome (set by declare-winner / declare-draw)
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>true</c> when a game-outcome primitive has fired.
    /// The session loop exits after the current post-action sequence completes.
    /// </summary>
    public bool GameIsOver { get; private set; }

    /// <summary>
    /// The winning player's name, or <c>null</c> for a draw.
    /// Meaningful only when <see cref="GameIsOver"/> is <c>true</c>.
    /// </summary>
    public string? PendingWinner { get; private set; }

    /// <summary>
    /// Records the game outcome.  Safe to call multiple times; first caller wins
    /// (subsequent calls are no-ops to avoid clobbering an already-decided result).
    /// </summary>
    public void DeclareOutcome(string? winner)
    {
        if (GameIsOver) return; // first declaration wins
        GameIsOver    = true;
        PendingWinner = winner;
    }

    // -----------------------------------------------------------------------
    //  Validation clone (D21)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a lightweight clone of the current state suitable for cost
    /// validation.  The clone is a shallow copy of:
    /// <list type="bullet">
    ///   <item>The atom table (with copied Accumulators, ZoneId, OwnerId, Kind)</item>
    ///   <item>Modifier index (deep-copy structure, shallow contribution refs)</item>
    ///   <item>Condition index (deep-copy structure, shallow contribution refs)</item>
    /// </list>
    /// Also copied (required for cost-body keyword resolution):
    /// <list type="bullet">
    ///   <item>Session atom ID — so <c>get-state(session(), ...)</c> works in cost bodies</item>
    ///   <item>Player name registry — so player lookups resolve in cost bodies</item>
    /// </list>
    /// Excluded (per D21):
    /// <list type="bullet">
    ///   <item><c>EventLog</c> — not part of <c>GameState</c></item>
    ///   <item><c>ActiveStaticEffects</c> and <c>DormantDeclarativeEffects</c></item>
    ///   <item><c>ContributionRegistry</c></item>
    ///   <item>Game outcome flags (<c>GameIsOver</c>, <c>PendingWinner</c>)</item>
    /// </list>
    /// The clone shares the same <see cref="AtomIdCounter"/> position; new
    /// allocations on the clone will not conflict with the original because
    /// the clone is discarded after validation.
    /// </summary>
    internal GameState CloneForValidation()
    {
        var clone = new GameState();

        // Copy session atom ID so get-state(session(), ...) works in cost bodies.
        clone.SessionAtomId = SessionAtomId;

        // Deep-copy atom snapshots — each atom gets its own Accumulators/ModifierIndex/ConditionIndex.
        foreach (var (id, src) in _atoms)
        {
            var dst = new AtomSnapshot
            {
                Id      = src.Id,
                Kind    = src.Kind,
                OwnerId = src.OwnerId,
                ZoneId  = src.ZoneId,
            };

            // Copy accumulators (value semantics).
            foreach (var (k, v) in src.Accumulators)
                dst.Accumulators[k] = v;

            // Copy modifier index (list of refs — contributions not tracked in clone).
            foreach (var (prop, mods) in src.ModifierIndex)
                dst.ModifierIndex[prop] = new List<ModifierContribution>(mods);

            // Copy condition index (list of refs — contributions not tracked in clone).
            foreach (var (cond, contribs) in src.ConditionIndex)
                dst.ConditionIndex[cond] = new List<ConditionContribution>(contribs);

            clone._atoms[id] = dst;
        }

        // Copy player name registry so player lookups work in cost bodies.
        foreach (var (atomId, name) in _playerNames)
            clone.RegisterPlayerName(atomId, name);

        // NOTE: ContributionRegistry, ActiveStaticEffects, DormantDeclarativeEffects,
        // and GameIsOver are NOT copied per D21 — the clone is read-only for those.
        // The ID counters start at 0 on the clone; any new contributions created
        // during validation will be discarded with the clone.
        return clone;
    }

    // -----------------------------------------------------------------------
    //  Snapshot capture and restore (D17)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Captures a <see cref="GameStateSnapshot"/> of the current settled state.
    /// Called at each turn boundary before any phase init block executes (D17
    /// Decision 1).
    /// </summary>
    /// <param name="gameDefinitionId">From <see cref="GameDefinition.Id"/>.</param>
    /// <param name="turnNumber">The 1-based turn number about to begin.</param>
    /// <param name="finalizedLog">The finalized event log at this point.</param>
    /// <param name="rng">The RNG state at this point.</param>
    /// <param name="atomDefinitionNames">
    /// Maps each atom ID to its definition name (e.g. card def name, zone def name).
    /// Null entries are omitted; session and player atoms have no definition name.
    /// </param>
    public GameStateSnapshot ToSnapshot(
        string gameDefinitionId,
        int    turnNumber,
        IReadOnlyList<GameEvent> finalizedLog,
        RngSnapshot rng,
        IReadOnlyDictionary<AtomId, string> atomDefinitionNames)
    {
        // Capture atoms.
        var atomData = _atoms.Values.Select(a => new AtomSnapshotData(
            Id:           a.Id,
            Kind:         a.Kind,
            RefName:      atomDefinitionNames.TryGetValue(a.Id, out var defName) ? defName : null,
            OwnerId:      a.OwnerId,
            ZoneId:       a.ZoneId,
            Accumulators: new Dictionary<string, double>(a.Accumulators)
        )).ToList();

        // Capture contributions from the registry.
        var contributions = ContributionRegistry.Values.Select<IContribution, ContributionSnapshot>(c =>
        {
            return c switch
            {
                ModifierContributionWrapper { Inner: var mc } =>
                    new ModifierContributionSnapshot(
                        mc.Id, mc.Source, mc.TargetAtom, mc.PropertyName,
                        mc.Kind, mc.Value, mc.Lifetime),
                ConditionContributionWrapper { Inner: var cc } =>
                    new ConditionContributionSnapshot(
                        cc.Id, cc.Source, cc.TargetAtom, cc.ConditionName, cc.Lifetime),
                _ => throw new InvalidOperationException(
                    $"Unknown contribution type: {c.GetType().Name}")
            };
        }).ToList();

        // Capture active static effects.
        var activeEffects = ActiveStaticEffects.Select(se =>
        {
            var snap = new StaticEffectSnapshot
            {
                Id                  = se.Id,
                IsDeclarative       = se.IsDeclarative,
                OwnerAtomId         = se.OwnerAtom,
                Lifetime            = se.Lifetime,
                TriggerFireCount    = se.TriggerFireCount,
                TriggerHighWaterMark = se.TriggerHighWaterMark,
                OwnedContributions  = se.OwnedContributions.ToList(),
                // Declarative effects are stored by reference; dynamic ones inline their trigger.
                DeclarativeRef = se.IsDeclarative && se.CardDefinitionName is not null
                    ? new StaticEffectDefRef(se.CardDefinitionName, se.EffectIndex)
                    : null,
                DynamicTrigger = !se.IsDeclarative ? se.Trigger : null,
            };
            snap.ValidateExclusive();
            return snap;
        }).ToList();

        // Capture dormant effects.
        var dormantEffects = DormantDeclarativeEffects
            .Where(d => d.CardDefinitionName is not null)
            .Select(d => new DormantEffectSnapshot(
                d.OwnerAtom,
                d.CardDefinitionName!,
                d.EffectIndex))
            .ToList();

        // Capture player name registry for restore.
        var playerNames = _playerNames.ToDictionary(
            kv => kv.Key.Value,
            kv => kv.Value);

        return new GameStateSnapshot(
            Version:            GameStateSnapshot.CurrentVersion,
            GameDefinitionId:   gameDefinitionId,
            TurnNumber:         turnNumber,
            NextAtomId:         _atomIds.PeekNext(),
            NextContributionId: _contribIds.PeekNext(),
            SessionAtomId:      SessionAtomId,
            Atoms:              atomData,
            Contributions:      contributions,
            ActiveStaticEffects: activeEffects,
            DormantEffects:     dormantEffects,
            PlayerNames:        playerNames,
            FinalizedLog:       finalizedLog,
            Rng:                rng);
    }

    /// <summary>
    /// Restores game state from a <see cref="GameStateSnapshot"/>.
    /// Populates <paramref name="atomDefinitionNames"/>,
    /// <paramref name="playerAtomIds"/>, and <paramref name="playerOrder"/>
    /// so <see cref="GameSession"/> can drive the action window as if the
    /// session had been provisioned normally.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <param name="definition">The game definition to use for resolving declarative effect refs.</param>
    /// <param name="atomDefinitionNames">Output: map from atom ID to definition name.</param>
    /// <param name="playerAtomIds">Output: map from player name to player atom ID.</param>
    /// <param name="playerOrder">Output: ordered list of player names (insertion order).</param>
    public void LoadFromSnapshot(
        GameStateSnapshot snapshot,
        GameDefinition definition,
        Dictionary<AtomId, string> atomDefinitionNames,
        Dictionary<string, AtomId> playerAtomIds,
        List<string> playerOrder)
    {
        // Restore ID counters so new allocations continue from where the
        // snapshot left off without collisions.
        _atomIds.Restore(snapshot.NextAtomId);
        _contribIds.Restore(snapshot.NextContributionId);
        SessionAtomId = snapshot.SessionAtomId;

        // Restore atoms.
        foreach (var data in snapshot.Atoms)
        {
            var atom = new AtomSnapshot
            {
                Id      = data.Id,
                Kind    = data.Kind,
                OwnerId = data.OwnerId,
                ZoneId  = data.ZoneId,
            };
            foreach (var (k, v) in data.Accumulators)
                atom.Accumulators[k] = v;

            _atoms[data.Id] = atom;

            // Rebuild _atomDefinitionNames from the RefName stored in the snapshot.
            if (data.RefName is not null)
                atomDefinitionNames[data.Id] = data.RefName;
        }

        // Restore player name registries and player order.
        foreach (var (idValue, name) in snapshot.PlayerNames)
        {
            var atomId = new AtomId(idValue);
            RegisterPlayerName(atomId, name);
            playerAtomIds[name] = atomId;
        }

        // Player order = insertion order of PlayerDefinitions (preserved across snapshot).
        foreach (var name in definition.PlayerDefinitions.Keys)
            if (playerAtomIds.ContainsKey(name))
                playerOrder.Add(name);

        // Restore contributions and build ModifierIndex / ConditionIndex per atom
        // (Decision 7: indices are NOT stored in the snapshot — reconstructed here).
        foreach (var cs in snapshot.Contributions)
        {
            switch (cs)
            {
                case ModifierContributionSnapshot m:
                {
                    var mc = new ModifierContribution(
                        m.Id, m.Source, m.TargetAtom, m.PropertyName,
                        m.Kind, m.Value, m.Lifetime);
                    var atom = GetAtom(m.TargetAtom);
                    if (!atom.ModifierIndex.TryGetValue(m.PropertyName, out var list))
                        atom.ModifierIndex[m.PropertyName] = list = new List<ModifierContribution>();
                    list.Add(mc);
                    ContributionRegistry[m.Id] = new ModifierContributionWrapper(mc);
                    break;
                }
                case ConditionContributionSnapshot c:
                {
                    var cc = new ConditionContribution(
                        c.Id, c.Source, c.TargetAtom, c.ConditionName, c.Lifetime);
                    var atom = GetAtom(c.TargetAtom);
                    if (!atom.ConditionIndex.TryGetValue(c.ConditionName, out var list))
                        atom.ConditionIndex[c.ConditionName] = list = new List<ConditionContribution>();
                    list.Add(cc);
                    ContributionRegistry[c.Id] = new ConditionContributionWrapper(cc);
                    break;
                }
            }
        }

        // Restore active static effects.
        foreach (var ses in snapshot.ActiveStaticEffects)
        {
            StaticEffectDef? sourceDef = null;
            string? cardDefName = null;
            int effectIndex = 0;

            if (ses.DeclarativeRef is not null)
            {
                // Resolve the StaticEffectDef from the definition.
                cardDefName = ses.DeclarativeRef.CardDefinitionName;
                effectIndex = ses.DeclarativeRef.EffectIndex;
                if (definition.CardDefinitions.TryGetValue(cardDefName, out var cardDef)
                    && effectIndex < cardDef.StaticEffects.Count)
                    sourceDef = cardDef.StaticEffects[effectIndex];
            }

            var se = new StaticEffect
            {
                Id                   = ses.Id,
                OwnerAtom            = ses.OwnerAtomId,
                IsDeclarative        = ses.IsDeclarative,
                SourceDefinition     = sourceDef,
                CardDefinitionName   = cardDefName,
                EffectIndex          = effectIndex,
                Lifetime             = ses.Lifetime,
                TriggerFireCount     = ses.TriggerFireCount,
                TriggerHighWaterMark = ses.TriggerHighWaterMark,
                Trigger              = sourceDef?.Trigger ?? ses.DynamicTrigger,
                ParameterModification = sourceDef?.ParameterModification,
            };
            se.OwnedContributions.AddRange(ses.OwnedContributions);
            ActiveStaticEffects.Add(se);
        }

        // Restore dormant effects.
        foreach (var ds in snapshot.DormantEffects)
        {
            if (!definition.CardDefinitions.TryGetValue(ds.CardDefinitionName, out var cardDef))
                continue; // unknown card definition — skip gracefully
            if (ds.EffectIndex >= cardDef.StaticEffects.Count)
                continue; // index out of range — skip gracefully

            DormantDeclarativeEffects.Add(new DormantDeclarativeEffect
            {
                OwnerAtom          = ds.OwnerAtomId,
                EffectDef          = cardDef.StaticEffects[ds.EffectIndex],
                CardDefinitionName = ds.CardDefinitionName,
                EffectIndex        = ds.EffectIndex,
            });
        }
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

    IReadOnlyDictionary<string, double> IGameStateReadable.GetAccumulators(AtomId atom) =>
        GetAtom(atom).Accumulators;

    IReadOnlyList<string> IGameStateReadable.GetActiveConditions(AtomId atom) =>
        GetAtom(atom).ConditionIndex
            .Where(kvp => kvp.Value.Count > 0)
            .Select(kvp => kvp.Key)
            .ToList();

    IReadOnlyList<string> IGameStateReadable.GetModifierKeys(AtomId atom) =>
        GetAtom(atom).ModifierIndex
            .Where(kvp => kvp.Value.Count > 0)
            .Select(kvp => kvp.Key)
            .ToList();
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

    /// <summary>
    /// For declarative effects: the name of the card definition that owns this
    /// static effect.  Stored at creation so snapshot serialization can produce
    /// a <see cref="StaticEffectDefRef"/> without scanning all card definitions.
    /// </summary>
    public string? CardDefinitionName { get; init; }

    /// <summary>
    /// For declarative effects: the 0-based index of this effect in
    /// <c>CardDefinition.StaticEffects</c>.  Paired with
    /// <see cref="CardDefinitionName"/> to form a <see cref="StaticEffectDefRef"/>.
    /// </summary>
    public int EffectIndex { get; init; }

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

    /// <summary>
    /// The card definition name for snapshot serialization (D17).
    /// Non-null when this effect was provisioned via manifest/card-creation.
    /// </summary>
    public string? CardDefinitionName { get; init; }

    /// <summary>
    /// 0-based index of this effect in <c>CardDefinition.StaticEffects</c>.
    /// Used together with <see cref="CardDefinitionName"/> to produce a
    /// <see cref="StaticEffectDefRef"/> during snapshot capture (D17).
    /// </summary>
    public int EffectIndex { get; init; }
}
