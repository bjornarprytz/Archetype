using Archetype.Core;

namespace Archetype.Engine;

// ---------------------------------------------------------------------------
//  GameSessionBuilder — fluent construction of a GameSession (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// Fluent builder for constructing a <see cref="GameSession"/>.
/// <para>
/// Obtain an instance via <c>GameSession.Create(definition)</c>.
/// </para>
/// </summary>
public sealed class GameSessionBuilder
{
    private readonly GameDefinition _definition;
    private readonly Dictionary<string, IPlayerStrategy> _strategies = new(StringComparer.Ordinal);
    private IRandomSource? _randomSource;
    private IEngineObserver? _observer;
    private HostManifest? _hostManifest;
    // Non-null when FromSavedState was called; drives the load path in Build().
    private GameStateSnapshot? _snapshot;

    internal GameSessionBuilder(GameDefinition definition) => _definition = definition;

    /// <summary>
    /// Registers an <see cref="IPlayerStrategy"/> for the named player.
    /// Must be called once for every player defined in
    /// <see cref="GameDefinition.PlayerDefinitions"/>.
    /// </summary>
    public GameSessionBuilder WithPlayerStrategy(string playerName, IPlayerStrategy strategy)
    {
        _strategies[playerName] = strategy;
        return this;
    }

    /// <summary>
    /// Sets the random source for the session.  Required.
    /// </summary>
    public GameSessionBuilder WithRandomSource(IRandomSource source)
    {
        _randomSource = source;
        return this;
    }

    /// <summary>
    /// Sets an optional <see cref="IEngineObserver"/> for cascade monitoring.
    /// </summary>
    public GameSessionBuilder WithObserver(IEngineObserver observer)
    {
        _observer = observer;
        return this;
    }

    /// <summary>
    /// Supplies an optional <see cref="HostManifest"/> that is appended after
    /// <see cref="GameDefinition.InitManifest"/> is provisioned.  Use this for
    /// session-specific additions such as a player's chosen cards.
    /// <para>
    /// Mutually exclusive with <see cref="FromSavedState"/>; calling both is a
    /// <see cref="SessionException"/> at <c>Build()</c> time.
    /// </para>
    /// </summary>
    public GameSessionBuilder WithHostManifest(HostManifest manifest)
    {
        _hostManifest = manifest;
        return this;
    }

    /// <summary>
    /// Loads a previously saved session from a <see cref="GameStateSnapshot"/>.
    /// Mutually exclusive with <see cref="WithHostManifest"/>; last call wins.
    /// <para>
    /// When this is set, <c>Build()</c> does not require <see cref="WithRandomSource"/>
    /// — the RNG is seeded from the snapshot.  <c>Build()</c> validates that
    /// <see cref="GameStateSnapshot.GameDefinitionId"/> matches
    /// <see cref="GameDefinition.Id"/> and throws <see cref="DefinitionException"/>
    /// on mismatch.
    /// </para>
    /// </summary>
    public GameSessionBuilder FromSavedState(GameStateSnapshot snapshot)
    {
        _snapshot = snapshot;
        return this;
    }

    /// <summary>
    /// Builds and returns a <see cref="GameSession"/> ready to run.
    /// </summary>
    /// <exception cref="DefinitionException">
    /// Thrown if <see cref="GameDefinition.Id"/> is null or empty, or if
    /// <see cref="FromSavedState"/> was called and the snapshot's
    /// <see cref="GameStateSnapshot.GameDefinitionId"/> does not match.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="WithRandomSource"/> was not called (and no snapshot
    /// is set), or if any player defined in
    /// <see cref="GameDefinition.PlayerDefinitions"/> does not have a
    /// corresponding strategy registered.
    /// </exception>
    public GameSession Build()
    {
        // D17: every definition must have a non-empty Id so snapshots can be
        // validated against the definition that produced them.
        if (string.IsNullOrEmpty(_definition.Id))
            throw new DefinitionException(
                "GameSessionBuilder.Build(): GameDefinition.Id must be set to a non-empty string. " +
                "This is required for save/load snapshot validation (D17).");

        // D29: validate InitManifest zone LocalId uniqueness.
        var initZoneIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var zone in _definition.InitManifest.Zones)
        {
            if (!initZoneIds.Add(zone.LocalId))
                throw new DefinitionException(
                    $"GameDefinition.InitManifest: duplicate zone LocalId '{zone.LocalId}'. " +
                    "Zone LocalIds must be unique within InitManifest.Zones.");
        }

        // D29: validate InitManifest card LocalId uniqueness (non-null values only).
        var initCardIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var card in _definition.InitManifest.Cards)
        {
            if (card.LocalId is not null && !initCardIds.Add(card.LocalId))
                throw new DefinitionException(
                    $"GameDefinition.InitManifest: duplicate card LocalId '{card.LocalId}'. " +
                    "Non-null card LocalIds must be unique within InitManifest.Cards.");
        }

        // When loading from a snapshot the RNG is derived from the snapshot;
        // WithRandomSource is not required.
        if (_snapshot is null && _randomSource is null)
            throw new InvalidOperationException(
                "GameSessionBuilder.Build(): WithRandomSource is required.");

        // Validate snapshot compatibility — must come from the same game definition.
        if (_snapshot is not null &&
            !string.Equals(_snapshot.GameDefinitionId, _definition.Id, StringComparison.Ordinal))
            throw new DefinitionException(
                $"GameSessionBuilder.Build(): snapshot GameDefinitionId '{_snapshot.GameDefinitionId}' " +
                $"does not match GameDefinition.Id '{_definition.Id}'. " +
                "Cannot load a snapshot from a different game definition.");

        // D29: WithHostManifest and FromSavedState are mutually exclusive.
        if (_snapshot is not null && _hostManifest is not null)
            throw new SessionException(
                "GameSessionBuilder.Build(): WithHostManifest and FromSavedState are mutually exclusive. " +
                "A saved-state session cannot accept a host manifest because provisioning was already completed " +
                "when the snapshot was taken.");

        // D29: validate HostManifest LocalId uniqueness against InitManifest zone LocalIds.
        if (_hostManifest is not null)
        {
            // Build the set of InitManifest zone LocalIds for collision checking.
            var existingZoneIds = new HashSet<string>(
                _definition.InitManifest.Zones.Select(z => z.LocalId),
                StringComparer.Ordinal);

            // No duplicates within HostManifest.Zones.
            var hostZoneIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var zone in _hostManifest.Zones)
            {
                if (!hostZoneIds.Add(zone.LocalId))
                    throw new SessionException(
                        $"GameSessionBuilder.Build(): duplicate zone LocalId '{zone.LocalId}' " +
                        "within HostManifest.Zones.");
                if (existingZoneIds.Contains(zone.LocalId))
                    throw new SessionException(
                        $"GameSessionBuilder.Build(): HostManifest zone LocalId '{zone.LocalId}' " +
                        "collides with an InitManifest zone LocalId. Zone LocalIds must be unique " +
                        "across both manifests.");
            }

            // No duplicate non-null card LocalIds within HostManifest.Cards.
            var hostCardIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var card in _hostManifest.Cards)
            {
                if (card.LocalId is not null && !hostCardIds.Add(card.LocalId))
                    throw new SessionException(
                        $"GameSessionBuilder.Build(): duplicate card LocalId '{card.LocalId}' " +
                        "within HostManifest.Cards.");
            }

            // D29 §5: AtomStateOverride CardTarget must refer to InitManifest cards,
            // not HostManifest cards.  Validate at Build() time before provisioning.
            var initCardLocalIds = new HashSet<string>(
                _definition.InitManifest.Cards
                    .Where(c => c.LocalId is not null)
                    .Select(c => c.LocalId!),
                StringComparer.Ordinal);

            foreach (var stateOverride in _hostManifest.StateOverrides)
            {
                if (stateOverride.Target is CardTarget ct)
                {
                    if (!initCardLocalIds.Contains(ct.LocalId))
                    {
                        // If the LocalId matches a HostManifest card, give a specific error.
                        bool isHostCard = hostCardIds.Contains(ct.LocalId);
                        if (isHostCard)
                            throw new SessionException(
                                $"GameSessionBuilder.Build(): AtomStateOverride CardTarget '{ct.LocalId}' " +
                                "targets a HostManifest card. State overrides may only target InitManifest " +
                                "cards (D29 §5). Configure host card state via CardSpec fields.");
                        throw new SessionException(
                            $"GameSessionBuilder.Build(): AtomStateOverride CardTarget '{ct.LocalId}' " +
                            "does not match any non-null CardSpec.LocalId in InitManifest.Cards.");
                    }
                }
            }
        }

        // Every defined player needs a strategy.
        foreach (var name in _definition.PlayerDefinitions.Keys)
        {
            if (!_strategies.ContainsKey(name))
                throw new InvalidOperationException(
                    $"GameSessionBuilder.Build(): player '{name}' has no strategy registered. " +
                    $"Call .WithPlayerStrategy(\"{name}\", ...).");
        }

        // No strategy for an undefined player (catches typos in player names).
        foreach (var name in _strategies.Keys)
        {
            if (!_definition.PlayerDefinitions.ContainsKey(name))
                throw new InvalidOperationException(
                    $"GameSessionBuilder.Build(): unknown player '{name}'. " +
                    "Not defined in GameDefinition.PlayerDefinitions.");
        }

        // Derive RNG: from snapshot on the load path, from caller on the fresh path.
        var rng = _snapshot is not null
            ? SeededRandom.FromSnapshot(_snapshot.Rng)
            : _randomSource!;

        return new GameSession(_definition, _strategies, rng, _observer, _hostManifest, _snapshot);
    }
}

// ---------------------------------------------------------------------------
//  GameSession — the runtime handle (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// The public runtime handle for a game.  Provisions atoms from
/// <see cref="GameDefinition.InitManifest"/> (and an optional
/// <see cref="HostManifest"/>), then drives the phase/turn loop until a
/// state-based rule declares an outcome.
/// <para>
/// Obtain an instance via <c>GameSession.Create(definition).With...().Build()</c>.
/// </para>
/// </summary>
public sealed class GameSession
{
    private readonly GameDefinition _definition;
    private readonly IReadOnlyDictionary<string, IPlayerStrategy> _strategies;
    private readonly IRandomSource _randomSource;
    private readonly IEngineObserver? _observer;
    private readonly HostManifest? _hostManifest;
    // Non-null when the session was constructed via FromSavedState (D17 load path).
    private readonly GameStateSnapshot? _loadSnapshot;

    // These are constructed once and shared across the whole session.
    private readonly GameState _state;
    private readonly EventLog _eventLog;
    private readonly ActionResolver _resolver;

    // Built during provisioning so the action window can resolve card/ability
    // choices back to EffectBlockDefs without re-scanning the whole definition.
    private readonly Dictionary<AtomId, string> _atomDefinitionNames = new();
    private readonly Dictionary<string, AtomId> _playerAtomIds       = new(StringComparer.Ordinal);

    // Ordered list of player names in insertion order, for round-robin active-player selection.
    private readonly List<string> _playerOrder = new();

    internal GameSession(
        GameDefinition definition,
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        IEngineObserver? observer,
        HostManifest? hostManifest,
        GameStateSnapshot? loadSnapshot = null)
    {
        _definition   = definition;
        _strategies   = strategies;
        _randomSource = randomSource;
        _observer     = observer;
        _hostManifest = hostManifest;
        _loadSnapshot = loadSnapshot;

        _state    = new GameState();
        _eventLog = new EventLog();
        _resolver = ActionResolver.Create(strategies, randomSource, definition, observer);
    }

    // -----------------------------------------------------------------------
    //  Factory entry point
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="GameSessionBuilder"/> for the given game definition.
    /// </summary>
    public static GameSessionBuilder Create(GameDefinition definition) => new(definition);

    // -----------------------------------------------------------------------
    //  RunAsync — the main game loop (D14)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provisions the manifest, then runs the phase/turn loop until a
    /// state-based rule declares an outcome via <c>declare-winner</c> or
    /// <c>declare-draw</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="GameResult"/> with the winner's name (or <c>null</c> for a
    /// draw) and the finalized event log.
    /// </returns>
    /// <remarks>
    /// The loop runs until a state-based rule calls <c>declare-winner</c> or
    /// <c>declare-draw</c>.  If the game definition contains no phases and no
    /// SBR that can end the game, <see cref="RunAsync"/> will loop indefinitely.
    /// Convergence is entirely the game creator's responsibility (per §1.7 /
    /// Open Item 7 in the domain model).
    /// </remarks>
    public async Task<GameResult> RunAsync(CancellationToken ct = default)
    {
        int turn;

        if (_loadSnapshot is not null)
        {
            // D17 load path: restore state from snapshot; skip provisioning.
            _state.LoadFromSnapshot(_loadSnapshot, _definition, _atomDefinitionNames, _playerAtomIds, _playerOrder);
            turn = _loadSnapshot.TurnNumber;
        }
        else
        {
            // Fresh path: provision session atom + init manifest.
            ProvisionSession();
            turn = 1;
        }

        while (true)
        {
            // Update the engine-managed turn-number accumulator on the session atom.
            SetSessionAccumulator("turn-number", turn);
            SetSessionAccumulator("phase-index",  0);

            // D17: notify observer with a snapshot before any phase init block for this turn.
            // The snapshot captures fully settled state (prior-turn processing is complete).
            if (_observer is not null)
            {
                var snapshot = CreateSnapshot(turn);
                await _observer.OnTurnStart(turn, snapshot);
            }

            _eventLog.OpenTurn();
            try
            {
                var outcome = await RunTurnAsync(turn, ct);
                if (outcome is not null)
                    // CloseTurn (in finally) commits remaining events before we build the result.
                    return outcome;
            }
            finally
            {
                _eventLog.CloseTurn();
            }

            turn++;
        }
    }

    // -----------------------------------------------------------------------
    //  Turn execution
    // -----------------------------------------------------------------------

    /// <summary>
    /// Runs all phases for one turn.  Returns a <see cref="GameResult"/> if
    /// the game ends during this turn, or <c>null</c> if the turn completes
    /// normally.
    /// </summary>
    private async Task<GameResult?> RunTurnAsync(int turn, CancellationToken ct)
    {
        var activePlayer = GetActivePlayer(turn);

        for (int phaseIdx = 0; phaseIdx < _definition.Phases.Count; phaseIdx++)
        {
            // Keep phase-index up to date for game-creator queries via get-state(session, "phase-index").
            SetSessionAccumulator("phase-index", phaseIdx);

            var phase = _definition.Phases[phaseIdx];

            // Phase Init block.
            if (phase.Init is not null)
            {
                await _resolver.ResolveAction(phase.Init, _state, _eventLog, activePlayer, turn);
                if (_state.GameIsOver) return BuildResult();
            }

            // Action window: active player takes actions until they pass or the game ends.
            await RunActionWindowAsync(activePlayer, turn, ct);
            if (_state.GameIsOver) return BuildResult();

            // Phase Cleanup block.
            if (phase.Cleanup is not null)
            {
                await _resolver.ResolveAction(phase.Cleanup, _state, _eventLog, activePlayer, turn);
                if (_state.GameIsOver) return BuildResult();
            }
        }

        return null; // turn completed normally
    }

    // -----------------------------------------------------------------------
    //  Action window
    // -----------------------------------------------------------------------

    /// <summary>
    /// Opens an action window for <paramref name="activePlayer"/>.
    /// Repeatedly asks the player for an action via
    /// <see cref="IPlayerStrategy.SelectActionAsync"/> until they pass or
    /// the game ends.
    /// </summary>
    private async Task RunActionWindowAsync(string activePlayer, int turn, CancellationToken ct)
    {
        var strategy  = _strategies[activePlayer];
        var stateView = new GameStateView(_state);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var available = ComputeAvailableActions(activePlayer);
            var action    = await strategy.SelectActionAsync(available, stateView, ct);

            // null or Pass ends the action window.  We still run the post-pass
            // sequence (SBRs, triggers, CheckLifetimes) so that effects that
            // trigger on "end of action" or "on pass" can fire.
            if (action is null or Pass)
            {
                await _resolver.ResolveAction(null, _state, _eventLog, activePlayer, turn);
                // D30: populate LastActionEvents after the action completes.
                stateView.SetLastActionEvents(_eventLog.LastActionEvents);
                return;
            }

            // Resolve the primary effect and the cost blocks separately.
            // Costs run with IsCostBody=true before the primary effect, all within
            // the same action scope (D23).
            var primaryBlock = TranslatePlayerAction(action);
            var costBlocks   = ResolveCostBlocks(action);

            await _resolver.ResolveAction(primaryBlock, _state, _eventLog, activePlayer, turn,
                costBlocks: costBlocks);
            // D30: populate LastActionEvents after the action completes.
            stateView.SetLastActionEvents(_eventLog.LastActionEvents);
            if (_state.GameIsOver) return;
        }
    }

    /// <summary>
    /// Returns the cost blocks (one <see cref="EffectBlockDef"/> per
    /// <see cref="CostDef"/>) for the given action in declaration order,
    /// or <c>null</c> when the action has no costs.
    /// </summary>
    private IReadOnlyList<EffectBlockDef>? ResolveCostBlocks(PlayerAction action)
    {
        IReadOnlyList<CostDef>? costs = action switch
        {
            PlayCard pc =>
                _atomDefinitionNames.TryGetValue(pc.Card, out var defName) &&
                _definition.CardDefinitions.TryGetValue(defName, out var cardDef)
                    ? cardDef.Cost
                    : null,
            ActivateAbility aa =>
                _atomDefinitionNames.TryGetValue(aa.Source, out var defName2) &&
                _definition.CardDefinitions.TryGetValue(defName2, out var cardDef2)
                    ? cardDef2.AdditionalEffects
                              .FirstOrDefault(e => string.Equals(e.Name, aa.EffectName, StringComparison.Ordinal))
                              ?.Cost
                    : null,
            _ => null,
        };

        if (costs is null || costs.Count == 0) return null;
        return costs.Select(c => c.Body).ToList();
    }

    // -----------------------------------------------------------------------
    //  Available-actions computation (simplified — D14 open gap noted)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the set of actions currently available to the active player.
    /// <para>
    /// Uses two independent passes (D19 steps 2 and 4):<br/>
    /// <b>Pass 1 — PlayCard candidates (D24):</b> iterates ALL card atoms regardless
    /// of owner.  If <see cref="GameDefinition.PlayableZoneNames"/> is non-empty,
    /// filters to cards in zones whose definition name is in the list (zone
    /// ownership is NOT checked — D24 removes the zone-owner predicate).  Then
    /// evaluates each card's <see cref="CardDefinition.ActivationCondition"/>.
    /// Games that want owner-restricted play add
    /// <c>ActivationCondition: Kw.OwnedByActivePlayer()</c>.<br/>
    /// <b>Pass 2 — Ability candidates (D24):</b> iterates ALL card atoms regardless
    /// of zone or owner.  Zone / ownership restrictions are expressed via the
    /// ability's own <c>ActivationCondition</c>.<br/>
    /// <c>Pass</c> is always included.<br/>
    /// <b>ValidateActionArgs (D22):</b> a delegate is captured over the current
    /// state snapshot so the caller can validate cost affordability without
    /// a direct reference to <see cref="GameSession"/>.
    /// </para>
    /// </summary>
    private AvailableActions ComputeAvailableActions(string activePlayer)
    {
        // Build a set of playable zone definition names for O(1) lookup.
        // null PlayableZoneNames = no zone filter (any zone is playable).
        var playableZoneDefNames = _definition.PlayableZoneNames is { Count: > 0 } names
            ? new HashSet<string>(names, StringComparer.Ordinal)
            : null;

        var playableCards    = new List<PlayableCardOption>();
        var activatableAbils = new List<ActivatableAbilityOption>();

        // ── Pass 1: PlayCard candidates (D24 — no ownership predicate) ───────
        // D24: iterate ALL card atoms.  Zone filter only checks definition name;
        // zone ownership is no longer a gate (ownership is expressed via ActivationCondition).
        foreach (var cardId in _state.GetAtoms(AtomKind.Card))
        {
            var card = _state.GetAtom(cardId);

            // --- Zone filter (D24 — definition name only, no zone-owner check) ---
            if (playableZoneDefNames is not null)
            {
                if (!_atomDefinitionNames.TryGetValue(card.ZoneId, out var zoneDefName)
                    || !playableZoneDefNames.Contains(zoneDefName))
                    continue; // card is in a non-playable zone (or no definition name)
            }

            if (!_atomDefinitionNames.TryGetValue(cardId, out var cardDefName)) continue;
            if (!_definition.CardDefinitions.TryGetValue(cardDefName, out var cardDef)) continue;

            // --- Activation condition (D19 / D24) ---
            // Evaluated pure (no mutation, no log).  The card atom is injected
            // as "source" per D13.  Games that want ownership filtering add
            // ActivationCondition: Kw.OwnedByActivePlayer() (D24).
            if (cardDef.ActivationCondition is not null)
            {
                var bindings = new Dictionary<string, object> { ["source"] = cardId };
                if (!_resolver.EvaluateCondition(cardDef.ActivationCondition, _state, bindings))
                    continue; // condition false — card not playable right now
            }

            playableCards.Add(new PlayableCardOption(cardId, Array.Empty<TargetSet>()));
        }

        // ── Pass 2: Ability candidates (D24 — all card atoms, no ownership guard) ──
        // D24: zone membership and ownership are irrelevant.  Ownership / zone
        // restrictions are expressed via the ability's own ActivationCondition.
        foreach (var cardId in _state.GetAtoms(AtomKind.Card))
        {
            if (!_atomDefinitionNames.TryGetValue(cardId, out var cardDefName2)) continue;
            if (!_definition.CardDefinitions.TryGetValue(cardDefName2, out var cardDef2)) continue;

            foreach (var ability in cardDef2.AdditionalEffects)
            {
                if (ability.ActivationCondition is not null)
                {
                    var bindings = new Dictionary<string, object> { ["source"] = cardId };
                    if (!_resolver.EvaluateCondition(ability.ActivationCondition, _state, bindings))
                        continue;
                }
                activatableAbils.Add(new ActivatableAbilityOption(cardId, ability.Name, Array.Empty<TargetSet>()));
            }
        }

        // D22: capture a snapshot of the current state for cost validation.
        // The delegate is safe to call multiple times before returning an action.
        // The snapshot is a lightweight clone (no event log, no static effects).
        var stateSnapshot       = _state.CloneForValidation();
        var strategiesSnapshot  = _strategies;   // immutable reference; safe to capture
        var randomSnapshot      = _randomSource; // read-only; safe to capture
        var defSnapshot         = _definition;   // immutable; safe to capture
        var activePlayerCapture = activePlayer;

        Func<PlayerAction, ValidationResult> validateDelegate = (action) =>
            CostValidator.Validate(
                costs: ResolveCostsForAction(action, defSnapshot),
                action: action,
                state:  stateSnapshot,
                definition: defSnapshot,
                strategies: strategiesSnapshot,
                randomSource: randomSnapshot,
                activePlayerName: activePlayerCapture);

        // Pass is always available — a player must never be stuck (D19 design D-D).
        return new AvailableActions(
            PlayableCards:        playableCards,
            ActivatableAbilities: activatableAbils,
            CanPass:              true,
            ValidateActionArgs:   validateDelegate);
    }

    /// <summary>
    /// Returns the cost list relevant for the given action, or null when the
    /// action type has no costs or the card/ability definition cannot be found.
    /// </summary>
    private IReadOnlyList<CostDef>? ResolveCostsForAction(
        PlayerAction action, GameDefinition def)
    {
        return action switch
        {
            PlayCard pc =>
                _atomDefinitionNames.TryGetValue(pc.Card, out var defName) &&
                def.CardDefinitions.TryGetValue(defName, out var cardDef)
                    ? cardDef.Cost
                    : null,
            ActivateAbility aa =>
                _atomDefinitionNames.TryGetValue(aa.Source, out var defName2) &&
                def.CardDefinitions.TryGetValue(defName2, out var cardDef2)
                    ? cardDef2.AdditionalEffects
                              .FirstOrDefault(e => string.Equals(e.Name, aa.EffectName, StringComparison.Ordinal))
                              ?.Cost
                    : null,
            _ => null,
        };
    }

    // -----------------------------------------------------------------------
    //  Player-action → EffectBlockDef translation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Translates a <see cref="PlayerAction"/> into an <see cref="EffectBlockDef"/>
    /// to be executed via <see cref="ActionResolver.ResolveAction"/>.
    /// Returns <c>null</c> for <see cref="Pass"/> or unrecognised action types.
    /// </summary>
    private EffectBlockDef? TranslatePlayerAction(PlayerAction action)
    {
        return action switch
        {
            PlayCard pc           => ResolveCardPrimaryEffect(pc.Card),
            ActivateAbility aa    => ResolveAbilityEffect(aa.Source, aa.EffectName),
            Pass                  => null,
            _                     => null,
        };
    }

    private EffectBlockDef? ResolveCardPrimaryEffect(AtomId cardAtomId)
    {
        if (!_atomDefinitionNames.TryGetValue(cardAtomId, out var defName)) return null;
        return _definition.CardDefinitions.TryGetValue(defName, out var def)
            ? def.PrimaryEffect
            : null;
    }

    private EffectBlockDef? ResolveAbilityEffect(AtomId sourceAtomId, string effectName)
    {
        if (!_atomDefinitionNames.TryGetValue(sourceAtomId, out var defName)) return null;
        if (!_definition.CardDefinitions.TryGetValue(defName, out var def)) return null;

        return def.AdditionalEffects
            .FirstOrDefault(e => string.Equals(e.Name, effectName, StringComparison.Ordinal))
            ?.Body;
    }

    // -----------------------------------------------------------------------
    //  Active player (round-robin by turn number)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the player name for the given turn, using round-robin rotation
    /// over <see cref="GameDefinition.PlayerDefinitions"/> insertion order.
    /// Turn 1 → first player, Turn 2 → second player (if any), etc.
    /// </summary>
    private string GetActivePlayer(int turn)
    {
        if (_playerOrder.Count == 0) return string.Empty;
        return _playerOrder[(turn - 1) % _playerOrder.Count];
    }

    // -----------------------------------------------------------------------
    //  Manifest provisioning (D14 §Provisioning order)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates the session atom, provisions players, then provisions the
    /// init manifest (if any).
    /// </summary>
    private void ProvisionSession()
    {
        // Step 1: session atom (engine-managed; initialised with turn-number=1, phase-index=0).
        var sessionId = _state.CreateSessionAtom();
        _state.GetAtom(sessionId).Accumulators["turn-number"] = 1;
        _state.GetAtom(sessionId).Accumulators["phase-index"] = 0;

        // Step 2: player atoms in insertion order.
        foreach (var (name, _) in _definition.PlayerDefinitions)
        {
            var playerId = _state.CreateAtom(AtomKind.Player);
            _state.RegisterPlayerName(playerId, name);
            _playerAtomIds[name] = playerId;
            _playerOrder.Add(name);
        }

        // Steps 3–6: InitManifest provisioning (D29: always applied, non-nullable).
        ProvisionManifest(_definition.InitManifest, out var localIdToAtomId);

        // Steps 7–9: HostManifest provisioning (optional append and patch layer).
        if (_hostManifest is not null)
            ProvisionHostManifest(_hostManifest, localIdToAtomId);
    }

    /// <summary>
    /// Provisions an <see cref="InitManifest"/> into <see cref="_state"/>.
    /// <para>
    /// Follows the nine-step provisioning order specified in D29:
    /// zones → cards (with static effects) → card overrides → player overrides.
    /// The <paramref name="localIdToAtomId"/> map is returned to the caller so that
    /// <see cref="ProvisionHostManifest"/> can resolve zone references that span
    /// both manifests.  No events are logged during provisioning.
    /// </para>
    /// </summary>
    private void ProvisionManifest(InitManifest manifest, out Dictionary<string, AtomId> localIdToAtomId)
    {
        // Zone LocalId → engine AtomId mapping; returned to caller for host manifest use.
        var zoneLocalToAtomId = new Dictionary<string, AtomId>(StringComparer.Ordinal);
        localIdToAtomId = zoneLocalToAtomId;

        // -- Step 3: zones --
        foreach (var zoneSpec in manifest.Zones)
        {
            var ownerId = RequirePlayerAtomId(zoneSpec.Owner, "ZoneSpec");
            var zoneId  = _state.CreateAtom(AtomKind.Zone, ownerId: ownerId);
            zoneLocalToAtomId[zoneSpec.LocalId] = zoneId;

            ApplyAccumulators(zoneId, zoneSpec.Accumulators);
            ApplyConditions(zoneId, zoneSpec.Conditions);

            // Track definition name so future zone-keyed lookups work.
            _atomDefinitionNames[zoneId] = zoneSpec.Definition;
        }

        // Lifetimes checker is needed to provision declarative static effects.
        var lifetimes = new LifetimeChecker(new BlockExecutor());

        // -- Step 4: cards --
        foreach (var cardSpec in manifest.Cards)
        {
            if (!zoneLocalToAtomId.TryGetValue(cardSpec.ZoneLocalId, out var zoneAtomId))
                throw new InvalidOperationException(
                    $"Card '{cardSpec.Definition}' references unknown zone LocalId '{cardSpec.ZoneLocalId}'.");

            var ownerId = RequirePlayerAtomId(cardSpec.Owner, "CardSpec");
            var cardId  = _state.CreateAtom(AtomKind.Card, ownerId: ownerId, zoneId: zoneAtomId);
            _atomDefinitionNames[cardId] = cardSpec.Definition;

            // D29: register card LocalId → AtomId so AtomStateOverride can target it.
            if (cardSpec.LocalId is not null)
                zoneLocalToAtomId[$"card:{cardSpec.LocalId}"] = cardId;

            // Provision declarative static effects from the card definition (D6/D12).
            // Pass cardDefinitionName and effectIndex so D17 snapshots can produce
            // StaticEffectDefRef without scanning all definitions.
            if (_definition.CardDefinitions.TryGetValue(cardSpec.Definition, out var cardDef))
            {
                for (int ei = 0; ei < cardDef.StaticEffects.Count; ei++)
                    lifetimes.ProvisionDeclarativeEffect(
                        cardDef.StaticEffects[ei], cardId, _state,
                        cardDefinitionName: cardSpec.Definition,
                        effectIndex: ei);
            }

            // -- Step 5: mutable state overrides --
            ApplyAccumulators(cardId, cardSpec.Accumulators);
            ApplyConditions(cardId, cardSpec.Conditions);
        }

        // -- Step 6: player mutable state overrides --
        foreach (var playerSpec in manifest.PlayerStates)
        {
            var playerId = RequirePlayerAtomId(playerSpec.Player, "PlayerStateSpec");
            ApplyAccumulators(playerId, playerSpec.Accumulators);
            ApplyConditions(playerId, playerSpec.Conditions);
        }
    }

    /// <summary>
    /// Provisions the host manifest's zones, cards, and state overrides into
    /// <see cref="_state"/> (steps 7–9 of D29's nine-step provisioning order).
    /// <para>
    /// The <paramref name="localIdToAtomId"/> map is already populated with zone
    /// and card LocalId → AtomId entries from <see cref="ProvisionManifest"/>
    /// (steps 3–6).  Host zones and cards extend it; state overrides patch atoms
    /// from either manifest.
    /// </para>
    /// </summary>
    private void ProvisionHostManifest(
        HostManifest host,
        Dictionary<string, AtomId> localIdToAtomId)
    {
        var lifetimes = new LifetimeChecker(new BlockExecutor());

        // -- Step 7: host zones --
        foreach (var zoneSpec in host.Zones)
        {
            var ownerId = RequirePlayerAtomId(zoneSpec.Owner, "HostManifest ZoneSpec");
            var zoneId  = _state.CreateAtom(AtomKind.Zone, ownerId: ownerId);
            localIdToAtomId[zoneSpec.LocalId] = zoneId;

            ApplyAccumulators(zoneId, zoneSpec.Accumulators);
            ApplyConditions(zoneId, zoneSpec.Conditions);
            _atomDefinitionNames[zoneId] = zoneSpec.Definition;
        }

        // -- Step 8: host cards --
        foreach (var cardSpec in host.Cards)
        {
            // ZoneLocalId may reference zones from either InitManifest or HostManifest.
            if (!localIdToAtomId.TryGetValue(cardSpec.ZoneLocalId, out var zoneAtomId))
                throw new InvalidOperationException(
                    $"HostManifest card '{cardSpec.Definition}' references unknown zone LocalId " +
                    $"'{cardSpec.ZoneLocalId}'. It must exist in either InitManifest or HostManifest zones.");

            var ownerId = RequirePlayerAtomId(cardSpec.Owner, "HostManifest CardSpec");
            var cardId  = _state.CreateAtom(AtomKind.Card, ownerId: ownerId, zoneId: zoneAtomId);
            _atomDefinitionNames[cardId] = cardSpec.Definition;

            // Register card LocalId for potential state override reference.
            if (cardSpec.LocalId is not null)
                localIdToAtomId[$"host-card:{cardSpec.LocalId}"] = cardId;

            // Provision declarative static effects.
            if (_definition.CardDefinitions.TryGetValue(cardSpec.Definition, out var cardDef))
            {
                for (int ei = 0; ei < cardDef.StaticEffects.Count; ei++)
                    lifetimes.ProvisionDeclarativeEffect(
                        cardDef.StaticEffects[ei], cardId, _state,
                        cardDefinitionName: cardSpec.Definition,
                        effectIndex: ei);
            }

            ApplyAccumulators(cardId, cardSpec.Accumulators);
            ApplyConditions(cardId, cardSpec.Conditions);
        }

        // -- Step 9: state overrides (accumulator merge, condition append) --
        // D29: AtomStateOverride may only target atoms from InitManifest, not HostManifest atoms.
        foreach (var stateOverride in host.StateOverrides)
        {
            var atomId = ResolveOverrideTarget(stateOverride.Target, localIdToAtomId);
            ApplyAccumulators(atomId, stateOverride.Accumulators);
            ApplyConditions(atomId, stateOverride.Conditions);
        }
    }

    /// <summary>
    /// Resolves an <see cref="OverrideTarget"/> to an <see cref="AtomId"/>.
    /// Throws <see cref="SessionException"/> if the target cannot be resolved or
    /// targets a <see cref="HostManifest"/>-provisioned atom.
    /// </summary>
    private AtomId ResolveOverrideTarget(
        OverrideTarget target,
        Dictionary<string, AtomId> localIdToAtomId)
    {
        switch (target)
        {
            case ZoneTarget z:
                if (!localIdToAtomId.TryGetValue(z.LocalId, out var zoneId))
                    throw new SessionException(
                        $"AtomStateOverride ZoneTarget '{z.LocalId}' does not match any zone LocalId " +
                        "in InitManifest or HostManifest zones.");
                // D29: overrides may target atoms from InitManifest zones.
                // (HostManifest zones are listed under the same namespace but this is accepted
                // by D29 §5 — the restriction is only on CardTarget, not ZoneTarget.)
                return zoneId;

            case CardTarget c:
                // D29: card overrides target InitManifest cards only (keyed with "card:" prefix).
                var initKey = $"card:{c.LocalId}";
                if (!localIdToAtomId.TryGetValue(initKey, out var cardId))
                {
                    // Check if it's a host card — targeting those is prohibited.
                    var hostKey = $"host-card:{c.LocalId}";
                    if (localIdToAtomId.ContainsKey(hostKey))
                        throw new SessionException(
                            $"AtomStateOverride CardTarget '{c.LocalId}' targets a HostManifest card. " +
                            "State overrides may only target InitManifest cards. Configure host card " +
                            "state via CardSpec.Accumulators / CardSpec.Conditions at construction time.");
                    throw new SessionException(
                        $"AtomStateOverride CardTarget '{c.LocalId}' does not match any non-null " +
                        "CardSpec.LocalId in InitManifest. Ensure the card has a LocalId set.");
                }
                return cardId;

            case PlayerTarget p:
                if (!_playerAtomIds.TryGetValue(p.PlayerName, out var playerId))
                    throw new SessionException(
                        $"AtomStateOverride PlayerTarget '{p.PlayerName}' does not match any player " +
                        "defined in GameDefinition.PlayerDefinitions.");
                return playerId;

            default:
                throw new SessionException(
                    $"Unknown OverrideTarget type '{target.GetType().Name}'.");
        }
    }

    // -----------------------------------------------------------------------
    //  Provisioning helpers
    // -----------------------------------------------------------------------

    private AtomId RequirePlayerAtomId(string playerName, string contextLabel)
    {
        if (!_playerAtomIds.TryGetValue(playerName, out var id))
            throw new InvalidOperationException(
                $"Manifest provisioning ({contextLabel}): player '{playerName}' is not defined " +
                "in GameDefinition.PlayerDefinitions.");
        return id;
    }

    private void ApplyAccumulators(AtomId atomId, IReadOnlyDictionary<string, double>? overrides)
    {
        if (overrides is null) return;
        var atom = _state.GetAtom(atomId);
        foreach (var (name, value) in overrides)
            atom.Accumulators[name] = value;
    }

    private void ApplyConditions(AtomId atomId, IReadOnlyList<string>? conditions)
    {
        if (conditions is null) return;
        var atom = _state.GetAtom(atomId);
        foreach (var cond in conditions)
        {
            var id = _state.NextContributionId();
            var contribution = new ConditionContribution(id, new AtomSource(atomId), atomId, cond);
            if (!atom.ConditionIndex.TryGetValue(cond, out var list))
                atom.ConditionIndex[cond] = list = new List<ConditionContribution>();
            list.Add(contribution);
            // Register in ContributionRegistry so ToSnapshot() captures this condition.
            // Matches the pattern in BuiltInHandlers.cs ApplyCondition handler (line 209).
            _state.ContributionRegistry[id] = new ConditionContributionWrapper(contribution);
        }
    }

    private void SetSessionAccumulator(string name, double value) =>
        _state.GetAtom(_state.SessionAtomId).Accumulators[name] = value;

    // -----------------------------------------------------------------------
    //  Snapshot creation (D17)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Captures a <see cref="GameStateSnapshot"/> at the current turn boundary.
    /// Called before each turn's first phase init block (D17 Decision 1 and 5).
    /// </summary>
    private GameStateSnapshot CreateSnapshot(int turnNumber)
    {
        // Only SeededRandom can produce a meaningful snapshot; other
        // IRandomSource implementations (e.g. MockRandomSource in tests) are
        // not snapshotable.  If the caller has not supplied a SeededRandom,
        // we emit a zero-state snapshot (Seed=0, CallCount=0) which signals
        // that RNG replay is not available.
        var rngSnapshot = _randomSource is SeededRandom sr
            ? sr.Snapshot()
            : new RngSnapshot(0, 0);

        return _state.ToSnapshot(
            gameDefinitionId: _definition.Id!,
            turnNumber:       turnNumber,
            finalizedLog:     _eventLog.Finalised,
            rng:              rngSnapshot,
            atomDefinitionNames: _atomDefinitionNames);
    }

    // -----------------------------------------------------------------------
    //  Result construction
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds the <see cref="GameResult"/> from the current game state and
    /// the finalized event log.  Should only be called after
    /// <see cref="EventLog.CloseTurn"/> has committed the current turn's events.
    /// </summary>
    private GameResult BuildResult() =>
        new(_state.PendingWinner, _eventLog.Finalised);
}
