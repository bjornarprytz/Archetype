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
    private InitManifest? _manifest;

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
    /// Uses <see cref="GameDefinition.DefaultInitManifest"/> as the starting
    /// state.  Mutually exclusive with <see cref="WithInitManifest(InitManifest)"/>
    /// and <see cref="FromSavedState"/>; last call wins.
    /// </summary>
    public GameSessionBuilder UseDefaultInit()
    {
        _manifest = _definition.DefaultInitManifest;
        return this;
    }

    /// <summary>
    /// Uses a custom <see cref="InitManifest"/> as the starting state.
    /// Mutually exclusive with <see cref="UseDefaultInit"/> and
    /// <see cref="FromSavedState"/>; last call wins.
    /// </summary>
    public GameSessionBuilder WithInitManifest(InitManifest manifest)
    {
        _manifest = manifest;
        return this;
    }

    /// <summary>
    /// Reserved for the save/load feature (D17).  Currently throws
    /// <see cref="NotSupportedException"/>.
    /// </summary>
    public GameSessionBuilder FromSavedState(object snapshot) =>
        throw new NotSupportedException(
            "Save/load (FromSavedState) is deferred per D17 and not yet implemented.");

    /// <summary>
    /// Builds and returns a <see cref="GameSession"/> ready to run.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="WithRandomSource"/> was not called, or if any
    /// player defined in <see cref="GameDefinition.PlayerDefinitions"/> does
    /// not have a corresponding strategy registered.
    /// </exception>
    public GameSession Build()
    {
        if (_randomSource is null)
            throw new InvalidOperationException(
                "GameSessionBuilder.Build(): WithRandomSource is required.");

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

        return new GameSession(_definition, _strategies, _randomSource, _observer, _manifest);
    }
}

// ---------------------------------------------------------------------------
//  GameSession — the runtime handle (D14)
// ---------------------------------------------------------------------------

/// <summary>
/// The public runtime handle for a game.  Provisions atoms from the
/// <see cref="InitManifest"/>, then drives the phase/turn loop until a
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
    private readonly InitManifest? _manifest;

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
        InitManifest? manifest)
    {
        _definition   = definition;
        _strategies   = strategies;
        _randomSource = randomSource;
        _observer     = observer;
        _manifest     = manifest;

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
        // Phase 1: provision session atom + init manifest.
        ProvisionSession();

        int turn = 1;

        while (true)
        {
            // Update the engine-managed turn-number accumulator on the session atom.
            SetSessionAccumulator("turn-number", turn);
            SetSessionAccumulator("phase-index",  0);

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
                return;
            }

            var block = TranslatePlayerAction(action);
            if (block is null) continue; // unknown action kind — skip silently

            await _resolver.ResolveAction(block, _state, _eventLog, activePlayer, turn);
            if (_state.GameIsOver) return;
        }
    }

    // -----------------------------------------------------------------------
    //  Available-actions computation (simplified — D14 open gap noted)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the set of actions currently available to the active player.
    /// <para>
    /// <b>Simplified implementation:</b> enumerates all cards owned by the
    /// player as playable (no activation-condition or cost-dry-run checks).
    /// Full evaluation of activation conditions, costs, and valid target sets
    /// is a known open gap; see <c>docs/implementation-status.md</c>.
    /// </para>
    /// </summary>
    private AvailableActions ComputeAvailableActions(string activePlayer)
    {
        if (!_playerAtomIds.TryGetValue(activePlayer, out var playerAtomId))
            return new AvailableActions(
                Array.Empty<PlayableCardOption>(),
                Array.Empty<ActivatableAbilityOption>(),
                CanPass: true);

        // Build a set of playable zone definition names for O(1) lookup.
        // null PlayableZoneNames = no zone filter (any zone is playable).
        var playableZoneDefNames = _definition.PlayableZoneNames is { Count: > 0 } names
            ? new HashSet<string>(names, StringComparer.Ordinal)
            : null;

        var playableCards    = new List<PlayableCardOption>();
        var activatableAbils = new List<ActivatableAbilityOption>();

        foreach (var cardId in _state.GetAtoms(AtomKind.Card))
        {
            var card = _state.GetAtom(cardId);
            if (card.OwnerId != playerAtomId) continue;

            // --- Zone filter (D19) ---
            // If PlayableZoneNames is set, the card must be in a zone whose
            // definition name appears in the list.
            if (playableZoneDefNames is not null)
            {
                if (!_atomDefinitionNames.TryGetValue(card.ZoneId, out var zoneDefName)
                    || !playableZoneDefNames.Contains(zoneDefName))
                    continue; // card is in a non-playable zone
            }

            if (!_atomDefinitionNames.TryGetValue(cardId, out var cardDefName)) continue;
            if (!_definition.CardDefinitions.TryGetValue(cardDefName, out var cardDef)) continue;

            // --- Activation condition (D19) ---
            // Evaluated pure (no mutation, no log).  The card atom is injected
            // as "source" per D13 — the ActivationCondition path has no StaticEffect
            // to provide it automatically, so we must supply it manually here.
            if (cardDef.ActivationCondition is not null)
            {
                var bindings = new Dictionary<string, object> { ["source"] = cardId };
                if (!_resolver.EvaluateCondition(cardDef.ActivationCondition, _state, bindings))
                    continue; // condition false — card not playable right now
            }

            playableCards.Add(new PlayableCardOption(cardId, Array.Empty<TargetSet>()));

            // --- Ability activation (D19) ---
            // Per the architect's decision (task 1.4): abilities use their own
            // ActivationCondition on NamedEffectBlockDef.  Zone restriction for
            // abilities is expressed via ActivationCondition (e.g. in-zone(source, ...))
            // rather than a separate zone list.
            foreach (var ability in cardDef.AdditionalEffects)
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

        // Pass is always available — a player must never be stuck (D19 design D-D).
        return new AvailableActions(
            PlayableCards:        playableCards,
            ActivatableAbilities: activatableAbils,
            CanPass:              true);
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

        // Steps 3–6: manifest provisioning (optional).
        if (_manifest is not null)
            ProvisionManifest(_manifest);
    }

    /// <summary>
    /// Provisions an <see cref="InitManifest"/> into <see cref="_state"/>.
    /// <para>
    /// Follows the provisioning order specified in D14:
    /// zones → cards (with static effects) → card overrides → player overrides.
    /// No events are logged during provisioning.
    /// </para>
    /// </summary>
    private void ProvisionManifest(InitManifest manifest)
    {
        // Zone LocalId → engine AtomId mapping; scoped to this provisioning pass.
        var zoneLocalToAtomId = new Dictionary<string, AtomId>(StringComparer.Ordinal);

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

            // Provision declarative static effects from the card definition (D6/D12).
            if (_definition.CardDefinitions.TryGetValue(cardSpec.Definition, out var cardDef))
            {
                foreach (var effectDef in cardDef.StaticEffects)
                    lifetimes.ProvisionDeclarativeEffect(effectDef, cardId, _state);
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
        }
    }

    private void SetSessionAccumulator(string name, double value) =>
        _state.GetAtom(_state.SessionAtomId).Accumulators[name] = value;

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
