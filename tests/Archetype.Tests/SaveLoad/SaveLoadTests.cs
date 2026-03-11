using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

// Fully qualify GameSession to avoid ambiguity with the Archetype.Tests.GameSession namespace.
using EngineGameSession = Archetype.Engine.GameSession;

namespace Archetype.Tests.SaveLoad;

/// <summary>
/// Tests for D17: save/load — <see cref="SeededRandom"/>,
/// <see cref="GameStateSnapshotSerializer"/>, <see cref="GameStateSnapshot"/>
/// round-trips, and the <see cref="GameSessionBuilder.FromSavedState"/> load path.
/// </summary>
public sealed class SaveLoadTests
{
    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Minimal <see cref="IEngineObserver"/> that captures snapshots supplied via
    /// <see cref="IEngineObserver.OnTurnStart"/> for test inspection.
    /// </summary>
    private sealed class CapturingObserver : IEngineObserver
    {
        private readonly List<(int Turn, GameStateSnapshot Snapshot)> _captured = new();

        /// <summary>All (turn, snapshot) pairs captured so far.</summary>
        public IReadOnlyList<(int Turn, GameStateSnapshot Snapshot)> Captured => _captured;

        public Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)
        {
            _captured.Add((turnNumber, snapshot));
            return Task.CompletedTask;
        }

        public Task<CascadeDirective> OnTriggerCascadeAsync(int iterationCount)
            => Task.FromResult(CascadeDirective.Continue);

        /// <inheritdoc/>
        public void OnDiagnostic(DiagnosticEvent e) { /* no-op in tests */ }
    }

    /// <summary>
    /// Builds a minimal single-player, single-phase <see cref="GameDefinition"/>
    /// (no cards, no zones) that declares a draw after exactly
    /// <paramref name="afterTurns"/> turns via an SBR.
    /// </summary>
    private static GameDefinition BuildDrawAfterTurnsDef(int afterTurns)
    {
        var drawBlock = new EffectBlockDef([
            new EffectBlockStep("declare-draw", Array.Empty<KeywordNode>()),
        ]);

        return new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw-after-turns",
                    Condition: new Invocation("at-least",
                        new Invocation("get-state", new Invocation("session"), new Literal("turn-number")),
                        new Literal((double)afterTurns)),
                    Body: drawBlock),
            },
            Phases:                 new List<PhaseDefinition>
            {
                new("main", Init: null, Cleanup: null),
            },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           InitManifest.Empty,
            Id:                     "save-load-test");
    }

    // -----------------------------------------------------------------------
    //  7.1 SeededRandom produces same sequence from same seed
    // -----------------------------------------------------------------------

    [Fact]
    public void SeededRandom_SameSeed_ProducesSameSequence()
    {
        var rng1 = new SeededRandom(12345L);
        var rng2 = new SeededRandom(12345L);

        for (int i = 0; i < 20; i++)
            Assert.Equal(
                rng1.NextInt(0, 100),
                rng2.NextInt(0, 100));
    }

    // -----------------------------------------------------------------------
    //  7.2 SeededRandom fast-forward via snapshot produces correct next value
    // -----------------------------------------------------------------------

    [Fact]
    public void SeededRandom_FastForward_ProducesCorrectNextValue()
    {
        const long seed = 99999L;
        var rng = new SeededRandom(seed);

        // Draw 10 values to advance the RNG.
        for (int i = 0; i < 10; i++)
            rng.NextInt(0, 100);

        // Capture snapshot now.
        var snapshot = rng.Snapshot();

        // Record the next value from the live RNG.
        int expected = rng.NextInt(0, 100);

        // Restore from snapshot and verify it produces the same value.
        var restored = SeededRandom.FromSnapshot(snapshot);
        int actual   = restored.NextInt(0, 100);

        Assert.Equal(expected, actual);
    }

    // -----------------------------------------------------------------------
    //  7.3 Snapshot round-trip preserves atom state
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Snapshot_RoundTrip_PreservesAtomState()
    {
        // Arrange: build a session and run turn 1 so the state is non-trivial.
        var def = BuildDrawAfterTurnsDef(afterTurns: 2);
        var observer = new CapturingObserver();

        var strategy = new ScriptedPlayerStrategy()
            .QueueAction(null)   // turn 1 pass
            .QueueAction(null);  // turn 2 pass → draw

        var session = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new SeededRandom(42L))
            .WithObserver(observer)
            .Build();

        await session.RunAsync();

        // At least two OnTurnStart calls should have been made.
        Assert.True(observer.Captured.Count >= 1);

        // Take the snapshot from turn 1 (index 0).
        var (turn, original) = observer.Captured[0];
        Assert.Equal(1, turn);

        // Act: round-trip through the serializer.
        var json    = GameStateSnapshotSerializer.Serialize(original);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        // Assert: core fields preserved.
        Assert.Equal(original.Version,          restored.Version);
        Assert.Equal(original.GameDefinitionId, restored.GameDefinitionId);
        Assert.Equal(original.TurnNumber,       restored.TurnNumber);
        Assert.Equal(original.NextAtomId,       restored.NextAtomId);
        Assert.Equal(original.SessionAtomId,    restored.SessionAtomId);

        // Atom count preserved.
        Assert.Equal(original.Atoms.Count, restored.Atoms.Count);

        // For each atom, accumulators should match.
        foreach (var origAtom in original.Atoms)
        {
            var resAtom = restored.Atoms.First(a => a.Id == origAtom.Id);
            foreach (var (key, val) in origAtom.Accumulators)
            {
                Assert.True(resAtom.Accumulators.TryGetValue(key, out var resVal));
                Assert.Equal(val, resVal);
            }
        }
    }

    // -----------------------------------------------------------------------
    //  7.4 Snapshot round-trip preserves active static effects (via build)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Snapshot_RoundTrip_PreservesActiveStaticEffects()
    {
        // Arrange: build a definition that has a card with a permanent static
        // effect so that there is at least one entry in ActiveStaticEffects.
        var modBlock = EffectBlockDef.Empty; // no state contribution block needed
        var staticDef = new StaticEffectDef(
            Lifetime:               LifetimeSpec.Permanent,
            StateContributionBlock: null,
            Trigger:                null);

        var cardDef = new CardDefinition(
            Name:                "buffed-card",
            StaticProperties:    new Dictionary<string, object>(),
            PrimaryEffect:       EffectBlockDef.Empty,
            AdditionalEffects:   new List<NamedEffectBlockDef>(),
            StaticEffects:       new List<StaticEffectDef> { staticDef },
            ActivationCondition: null);

        var zoneLocalId = "hand";
        var zoneDef = new ZoneDefinition(Name: "hand", StaticProperties: new Dictionary<string, object>());
        var manifest = new InitManifest(
            Zones:        new List<ZoneSpec>
            {
                new ZoneSpec(LocalId: zoneLocalId, Owner: "p1", Definition: "hand"),
            },
            Cards:        new List<CardSpec>
            {
                new CardSpec(Owner: "p1", ZoneLocalId: zoneLocalId, Definition: "buffed-card"),
            },
            PlayerStates: new List<PlayerStateSpec>());

        var drawBlock = new EffectBlockDef([
            new EffectBlockStep("declare-draw", Array.Empty<KeywordNode>()),
        ]);
        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["buffed-card"] = cardDef,
            },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = zoneDef,
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw",
                    Condition: new Literal(true),
                    Body: drawBlock),
            },
            Phases:                 new List<PhaseDefinition>
            {
                new("main", Init: null, Cleanup: null),
            },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:    manifest,
            Id:                     "save-load-test");

        var observer = new CapturingObserver();
        var strategy = new ScriptedPlayerStrategy().QueueAction(null);

        var session = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new SeededRandom(1L))
            .WithObserver(observer)
            .Build();

        await session.RunAsync();

        Assert.True(observer.Captured.Count >= 1);
        var original = observer.Captured[0].Snapshot;

        // Verify at least one active effect was captured.
        Assert.True(original.ActiveStaticEffects.Count >= 1);

        // Round-trip.
        var json     = GameStateSnapshotSerializer.Serialize(original);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        Assert.Equal(original.ActiveStaticEffects.Count, restored.ActiveStaticEffects.Count);
        var origEffect = original.ActiveStaticEffects[0];
        var restEffect = restored.ActiveStaticEffects[0];
        Assert.Equal(origEffect.Id,            restEffect.Id);
        Assert.Equal(origEffect.OwnerAtomId,   restEffect.OwnerAtomId);
        Assert.Equal(origEffect.IsDeclarative, restEffect.IsDeclarative);
        Assert.Equal(origEffect.DeclarativeRef?.CardDefinitionName,
                     restEffect.DeclarativeRef?.CardDefinitionName);
    }

    // -----------------------------------------------------------------------
    //  7.5 Snapshot round-trip: BoundArgs AtomId preserves type
    // -----------------------------------------------------------------------

    [Fact]
    public void Snapshot_RoundTrip_BoundArgs_AtomIdPreservesType()
    {
        // Build a snapshot that has a GameEvent with an AtomId in BoundArgs.
        var atomId    = new AtomId(42);
        var ev        = new GameEvent
        {
            SequenceNumber = 1,
            KeywordName    = "test-keyword",
            BoundArgs      = new Dictionary<string, object> { ["target"] = atomId },
        };
        var snapshot = BuildMinimalSnapshot("save-load-test", 1, new[] { ev });

        // Round-trip.
        var json     = GameStateSnapshotSerializer.Serialize(snapshot);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        Assert.Single(restored.FinalizedLog);
        var restoredEvent = restored.FinalizedLog[0];
        Assert.True(restoredEvent.BoundArgs.TryGetValue("target", out var restoredTarget));
        Assert.IsType<AtomId>(restoredTarget);
        Assert.Equal(atomId, (AtomId)restoredTarget!);
    }

    // -----------------------------------------------------------------------
    //  7.6 Snapshot round-trip: BoundArgs EventRef resolves correctly
    // -----------------------------------------------------------------------

    [Fact]
    public void Snapshot_RoundTrip_BoundArgs_EventRefResolvesCorrectly()
    {
        // Build a snapshot where one event has an EventRef in BoundArgs
        // pointing to another event.
        var innerEvent = new GameEvent
        {
            SequenceNumber = 1,
            KeywordName    = "inner-keyword",
            BoundArgs      = new Dictionary<string, object>(),
        };
        var outerEvent = new GameEvent
        {
            SequenceNumber = 2,
            KeywordName    = "outer-keyword",
            BoundArgs      = new Dictionary<string, object>
            {
                ["source-event"] = new EventRef(innerEvent),
            },
        };
        var snapshot = BuildMinimalSnapshot("save-load-test", 1, new[] { innerEvent, outerEvent });

        // Round-trip.
        var json     = GameStateSnapshotSerializer.Serialize(snapshot);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        Assert.Equal(2, restored.FinalizedLog.Count);

        var restoredOuter = restored.FinalizedLog.First(e => e.SequenceNumber == 2);
        Assert.True(restoredOuter.BoundArgs.TryGetValue("source-event", out var restoredRef));
        var eventRef = Assert.IsType<EventRef>(restoredRef);
        Assert.Equal(1, eventRef.Event.SequenceNumber);
        Assert.Equal("inner-keyword", eventRef.Event.KeywordName);
    }

    // -----------------------------------------------------------------------
    //  7.7 FromSavedState resumes at the correct turn
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FromSavedState_ResumesAtCorrectTurn()
    {
        // Arrange: run a game to turn 2 capturing the turn-2 snapshot, then
        // reload from it and verify the game resumes from turn 2.
        var def = BuildDrawAfterTurnsDef(afterTurns: 3);
        var observer = new CapturingObserver();

        // First run: 3 passes → game ends at turn 3 (after declare-draw).
        var strategy1 = new ScriptedPlayerStrategy()
            .QueueAction(null)
            .QueueAction(null)
            .QueueAction(null);

        var session1 = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy1)
            .WithRandomSource(new SeededRandom(77L))
            .WithObserver(observer)
            .Build();

        await session1.RunAsync();

        // Capture the turn-2 snapshot (index 1 in Captured).
        Assert.True(observer.Captured.Count >= 2,
            $"Expected at least 2 OnTurnStart calls but got {observer.Captured.Count}.");
        var turn2Snapshot = observer.Captured[1].Snapshot;
        Assert.Equal(2, turn2Snapshot.TurnNumber);

        // Second run: load from turn 2. The game should end after 2 more passes
        // (turn 2 action window, then turn 3 action window triggers the SBR).
        var observer2 = new CapturingObserver();
        var strategy2 = new ScriptedPlayerStrategy()
            .QueueAction(null)   // turn 2 pass
            .QueueAction(null);  // turn 3 pass → SBR threshold reached → declare-draw

        var session2 = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy2)
            .WithObserver(observer2)
            .FromSavedState(turn2Snapshot)
            .Build();

        var result = await session2.RunAsync();

        // The game should have ended via draw.
        Assert.True(result.IsDraw);

        // OnTurnStart should have been called for turn 2 (the loaded turn).
        Assert.True(observer2.Captured.Count >= 1);
        Assert.Equal(2, observer2.Captured[0].Turn);
    }

    // -----------------------------------------------------------------------
    //  7.8 FromSavedState with wrong GameDefinitionId throws DefinitionException
    // -----------------------------------------------------------------------

    [Fact]
    public void FromSavedState_GameDefinitionIdMismatch_ThrowsDefinitionException()
    {
        var def      = BuildDrawAfterTurnsDef(afterTurns: 1);
        var strategy = new ScriptedPlayerStrategy();

        var wrongSnapshot = BuildMinimalSnapshot("WRONG-GAME-ID", 1, Array.Empty<GameEvent>());

        Assert.Throws<DefinitionException>(() =>
            EngineGameSession.Create(def)
                .WithPlayerStrategy("p1", strategy)
                .FromSavedState(wrongSnapshot)
                .Build());
    }

    // -----------------------------------------------------------------------
    //  7.9 FromSavedState does not require WithRandomSource
    // -----------------------------------------------------------------------

    [Fact]
    public void FromSavedState_DoesNotRequireWithRandomSource()
    {
        var def      = BuildDrawAfterTurnsDef(afterTurns: 1);
        var strategy = new ScriptedPlayerStrategy().QueueAction(null);

        // Build a minimal snapshot for this game.
        var snapshot = BuildMinimalSnapshot("save-load-test", 1, Array.Empty<GameEvent>());

        // Build() should NOT throw even though WithRandomSource was never called.
        var session = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .FromSavedState(snapshot)
            .Build();

        Assert.NotNull(session);
    }

    // -----------------------------------------------------------------------
    //  7.10 OnTurnStart is called before the first phase Init block
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OnTurnStart_CalledBeforeFirstPhaseInit()
    {
        // Arrange: create a phase with an Init block that modifies an accumulator.
        // The observer should see the snapshot BEFORE that modification.
        var incrSessionBlock = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("marker"),
                new Literal(1.0),
            ]),
        ]);
        var drawBlock = new EffectBlockDef([
            new EffectBlockStep("declare-draw", Array.Empty<KeywordNode>()),
        ]);

        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw",
                    Condition: new Literal(true),
                    Body: drawBlock),
            },
            Phases:                 new List<PhaseDefinition>
            {
                // Init block increments "marker" on the session atom.
                new("main", Init: incrSessionBlock, Cleanup: null),
            },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           InitManifest.Empty,
            Id: "save-load-test");

        var observer = new CapturingObserver();
        var strategy = new ScriptedPlayerStrategy().QueueAction(null);

        var session = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new SeededRandom(1L))
            .WithObserver(observer)
            .Build();

        await session.RunAsync();

        Assert.True(observer.Captured.Count >= 1);

        // The snapshot must have been taken BEFORE the Init block executed.
        // So the "marker" accumulator should NOT be present (or should be 0).
        var snap = observer.Captured[0].Snapshot;
        var sessionAtom = snap.Atoms.First(a => a.Kind == AtomKind.Session);
        Assert.False(
            sessionAtom.Accumulators.TryGetValue("marker", out var markerVal) && markerVal > 0,
            "OnTurnStart snapshot should be taken before phase Init block executes.");
    }

    // -----------------------------------------------------------------------
    //  7.11 GameDefinitionBuilder.Build throws when Id is missing
    // -----------------------------------------------------------------------

    [Fact]
    public void GameDefinitionBuilder_Build_ThrowsWhenIdMissing()
    {
        // Create a definition WITHOUT an Id (null).
        var defNoId = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>(),
            Phases:                 new List<PhaseDefinition>(),
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            // D29: InitManifest is required; use Empty to satisfy the record constructor.
            InitManifest:           InitManifest.Empty
            // Id intentionally omitted — defaults to null
        );

        Assert.Throws<DefinitionException>(() =>
            EngineGameSession.Create(defNoId)
                .WithPlayerStrategy("p1", new ScriptedPlayerStrategy())
                .WithRandomSource(new SeededRandom(1L))
                .Build());
    }

    // -----------------------------------------------------------------------
    //  7.12 ModifierIndex is reconstructed correctly after load
    // -----------------------------------------------------------------------

    [Fact]
    public void ModifierIndex_ReconstructedCorrectly_AfterLoad()
    {
        // Build a snapshot that has a modifier contribution and verify that
        // after loading it, the computed property reflects the modifier.
        var targetAtomId = new AtomId(10);
        var contribId    = new ContributionId(100);

        var modContrib = new ModifierContributionSnapshot(
            Id:           contribId,
            Source:       new AtomSource(targetAtomId),
            TargetAtom:   targetAtomId,
            PropertyName: "strength",
            Kind:         ModifierKind.Additive,
            Value:        5.0,
            Lifetime:     null);

        var atomData = new AtomSnapshotData(
            Id:           targetAtomId,
            Kind:         AtomKind.Card,
            RefName:      null,
            OwnerId:      AtomId.None,
            ZoneId:       AtomId.None,
            Accumulators: new Dictionary<string, double> { ["strength"] = 10.0 });

        // Session atom is needed.
        var sessionAtomId = new AtomId(1);
        var sessionData   = new AtomSnapshotData(
            Id:           sessionAtomId,
            Kind:         AtomKind.Session,
            RefName:      null,
            OwnerId:      AtomId.None,
            ZoneId:       AtomId.None,
            Accumulators: new Dictionary<string, double> { ["turn-number"] = 1.0, ["phase-index"] = 0.0 });

        var snapshot = new GameStateSnapshot(
            Version:             GameStateSnapshot.CurrentVersion,
            GameDefinitionId:    "save-load-test",
            TurnNumber:          1,
            NextAtomId:          11,
            NextContributionId:  101,
            SessionAtomId:       sessionAtomId,
            Atoms:               new List<AtomSnapshotData> { sessionData, atomData },
            Contributions:       new List<ContributionSnapshot> { modContrib },
            ActiveStaticEffects: new List<StaticEffectSnapshot>(),
            DormantEffects:      new List<DormantEffectSnapshot>(),
            PlayerNames:         new Dictionary<long, string>(),
            FinalizedLog:        new List<GameEvent>(),
            Rng:                 new RngSnapshot(0L, 0L));

        // Round-trip through serializer.
        var json     = GameStateSnapshotSerializer.Serialize(snapshot);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        // Load into a fresh game definition.
        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>(),
            Phases:                 new List<PhaseDefinition> { new("main", null, null) },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>(),
            InitManifest:           InitManifest.Empty,
            Id:                     "save-load-test");

        // Load state using GameState.LoadFromSnapshot.
        var state                = new GameState();
        var atomDefNames         = new Dictionary<AtomId, string>();
        var playerAtomIds        = new Dictionary<string, AtomId>(StringComparer.Ordinal);
        var playerOrder          = new List<string>();
        state.LoadFromSnapshot(restored, def, atomDefNames, playerAtomIds, playerOrder);

        // Verify the computed property includes the modifier.
        // Base accumulator = 10.0, additive modifier = 5.0 → computed = 15.0
        double computed = state.GetAtom(targetAtomId).GetComputedProperty("strength");
        Assert.Equal(15.0, computed);
    }

    // -----------------------------------------------------------------------
    //  7.13 Manifest-provisioned conditions survive snapshot round-trip
    // -----------------------------------------------------------------------

    /// <summary>
    /// Regression test for BLOCKER 1: <see cref="GameSession.ApplyConditions"/> must
    /// register <see cref="ConditionContribution"/> objects in
    /// <see cref="GameState.ContributionRegistry"/> so that <c>ToSnapshot()</c>
    /// captures them. Before the fix, conditions set via <see cref="CardSpec.Conditions"/>
    /// were written to <c>ConditionIndex</c> but omitted from the registry, making them
    /// invisible to the snapshot serializer and causing <c>HasCondition</c> to return
    /// false after a load.
    /// </summary>
    [Fact]
    public async Task ManifestProvisionedCondition_SurvivesSnapshotRoundTrip()
    {
        // Arrange: a game with a card that starts with a condition applied via manifest.
        const string conditionName = "poisoned";

        var cardDef = new CardDefinition(
            Name:                "test-card",
            StaticProperties:    new Dictionary<string, object>(),
            PrimaryEffect:       EffectBlockDef.Empty,
            AdditionalEffects:   new List<NamedEffectBlockDef>(),
            StaticEffects:       new List<StaticEffectDef>(),
            ActivationCondition: null);

        var zoneDef = new ZoneDefinition(Name: "hand", StaticProperties: new Dictionary<string, object>());

        var manifest = new InitManifest(
            Zones: new List<ZoneSpec>
            {
                new ZoneSpec(LocalId: "hand", Owner: "p1", Definition: "hand"),
            },
            Cards: new List<CardSpec>
            {
                // The card starts the game with the "poisoned" condition.
                new CardSpec(
                    Owner:      "p1",
                    ZoneLocalId: "hand",
                    Definition:  "test-card",
                    Conditions:  new List<string> { conditionName }),
            },
            PlayerStates: new List<PlayerStateSpec>());

        var drawBlock = new EffectBlockDef([
            new EffectBlockStep("declare-draw", Array.Empty<KeywordNode>()),
        ]);

        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition> { ["test-card"] = cardDef },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition> { ["hand"] = zoneDef },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw", Condition: new Literal(true), Body: drawBlock),
            },
            Phases:                 new List<PhaseDefinition> { new("main", Init: null, Cleanup: null) },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:    manifest,
            Id:                     "save-load-test");

        var observer = new CapturingObserver();
        var strategy = new ScriptedPlayerStrategy().QueueAction(null);

        var session = EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new SeededRandom(1L))
            .WithObserver(observer)
            .Build();

        await session.RunAsync();

        Assert.True(observer.Captured.Count >= 1,
            "Expected at least one OnTurnStart call.");

        var original = observer.Captured[0].Snapshot;

        // Act: round-trip the snapshot through the serializer.
        var json     = GameStateSnapshotSerializer.Serialize(original);
        var restored = GameStateSnapshotSerializer.Deserialize(json);

        // The condition contribution must be present in the snapshot.
        Assert.True(
            restored.Contributions.OfType<ConditionContributionSnapshot>()
                    .Any(c => c.ConditionName == conditionName),
            $"Snapshot should contain a ConditionContributionSnapshot for '{conditionName}'. " +
            "If missing, ApplyConditions did not register it in ContributionRegistry.");

        // Load the snapshot into a fresh GameState and verify HasCondition.
        var loadedState    = new GameState();
        var atomDefNames   = new Dictionary<AtomId, string>();
        var playerAtomIds  = new Dictionary<string, AtomId>(StringComparer.Ordinal);
        var playerOrder    = new List<string>();
        loadedState.LoadFromSnapshot(restored, def, atomDefNames, playerAtomIds, playerOrder);

        // Find the card atom and assert the condition survived.
        var cardAtomId = loadedState.GetAtoms(AtomKind.Card).First();
        Assert.True(
            loadedState.GetAtom(cardAtomId).HasCondition(conditionName),
            $"HasCondition(\"{conditionName}\") should be true after loading a snapshot " +
            "that was created from a session provisioned with that condition via CardSpec.");
    }

    // -----------------------------------------------------------------------
    //  Internal helper: build a minimal GameStateSnapshot for unit tests
    // -----------------------------------------------------------------------

    private static GameStateSnapshot BuildMinimalSnapshot(
        string gameDefinitionId,
        int turnNumber,
        IReadOnlyList<GameEvent> log)
    {
        var sessionAtomId = new AtomId(1);
        return new GameStateSnapshot(
            Version:             GameStateSnapshot.CurrentVersion,
            GameDefinitionId:    gameDefinitionId,
            TurnNumber:          turnNumber,
            NextAtomId:          2,
            NextContributionId:  1,
            SessionAtomId:       sessionAtomId,
            Atoms:               new List<AtomSnapshotData>
            {
                new AtomSnapshotData(
                    Id:           sessionAtomId,
                    Kind:         AtomKind.Session,
                    RefName:      null,
                    OwnerId:      AtomId.None,
                    ZoneId:       AtomId.None,
                    Accumulators: new Dictionary<string, double>
                    {
                        ["turn-number"] = turnNumber,
                        ["phase-index"] = 0.0,
                    }),
            },
            Contributions:       new List<ContributionSnapshot>(),
            ActiveStaticEffects: new List<StaticEffectSnapshot>(),
            DormantEffects:      new List<DormantEffectSnapshot>(),
            PlayerNames:         new Dictionary<long, string>(),
            FinalizedLog:        log,
            Rng:                 new RngSnapshot(42L, 0L));
    }
}
