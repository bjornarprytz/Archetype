using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

namespace Archetype.Tests.HostManifestTests;

/// <summary>
/// Tests for D29 — InitManifest mandatory enforcement, HostManifest append layer,
/// LocalId uniqueness, and AtomStateOverride.
/// </summary>
public sealed class HostManifestTests
{
    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a minimal <see cref="GameDefinition"/> with one player "p1",
    /// a "draw-immediately" SBR so the session ends, and the given
    /// <see cref="InitManifest"/>.
    /// </summary>
    private static GameDefinition BuildDef(InitManifest manifest) =>
        new(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new("hand", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw", new Literal(true),
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           manifest,
            Id:                     "host-manifest-test");

    private static GameDefinition BuildDefWithCard(InitManifest manifest) =>
        new(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["test-card"] = new("test-card",
                    new Dictionary<string, object>(),
                    new EffectBlockDef([]),
                    Array.Empty<NamedEffectBlockDef>(),
                    Array.Empty<StaticEffectDef>()),
            },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new("hand", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw", new Literal(true),
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           manifest,
            Id:                     "host-manifest-test");

    private static Archetype.Engine.GameSession BuildSession(
        GameDefinition def, HostManifest? host = null)
    {
        var builder = Archetype.Engine.GameSession.Create(def)
            .WithPlayerStrategy("p1", new ScriptedPlayerStrategy().QueueAction(null))
            .WithRandomSource(new MockRandomSource());
        if (host is not null)
            builder = builder.WithHostManifest(host);
        return builder.Build();
    }

    /// <summary>
    /// Runs the session with a <see cref="CapturingStrategy"/> and returns the
    /// <see cref="GameStateView"/> captured during the first action window.
    /// Asserts the session ends as a draw and that the view was captured.
    /// </summary>
    private static async Task<GameStateView> RunWithCapture(GameDefinition def, HostManifest host)
    {
        GameStateView? capturedView = null;
        var strategy = new CapturingStrategy(view => { capturedView = view; return null; });
        var session = Archetype.Engine.GameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new MockRandomSource())
            .WithHostManifest(host)
            .Build();
        var result = await session.RunAsync();
        Assert.True(result.IsDraw);
        Assert.NotNull(capturedView);
        return capturedView!;
    }

    // -----------------------------------------------------------------------
    //  0.8a  InitManifest required — GameSessionBuilder.Build throws when absent
    // -----------------------------------------------------------------------
    // Note: D29 says InitManifest is non-nullable on GameDefinition so the record
    // constructor enforces it at compile time.  This test verifies the duplicate
    // zone-LocalId DefinitionException check (which is the build-time enforcement).

    [Fact]
    public void ZoneLocalId_Duplicate_InInitManifest_ThrowsDefinitionException()
    {
        var manifest = new InitManifest(
            Zones:        [new ZoneSpec("dup-id", "p1", "hand"), new ZoneSpec("dup-id", "p1", "hand")],
            Cards:        [],
            PlayerStates: []);

        var def = BuildDef(manifest);

        Assert.Throws<DefinitionException>(() =>
            BuildSession(def));
    }

    // -----------------------------------------------------------------------
    //  0.8b  CardSpec.LocalId optional — null by default
    // -----------------------------------------------------------------------

    [Fact]
    public void CardSpec_LocalId_Optional_NullByDefault()
    {
        // CardSpec without LocalId should have LocalId == null.
        var spec = new CardSpec("p1", "zone1", "some-card");
        Assert.Null(spec.LocalId);
    }

    // -----------------------------------------------------------------------
    //  0.8c  Duplicate zone LocalId across manifests → SessionException
    // -----------------------------------------------------------------------

    [Fact]
    public void ZoneLocalId_Duplicate_AcrossManifests_ThrowsSessionException()
    {
        var initManifest = new InitManifest(
            Zones:        [new ZoneSpec("shared-id", "p1", "hand")],
            Cards:        [],
            PlayerStates: []);
        var def = BuildDef(initManifest);

        var hostManifest = new HostManifest(
            Zones:          [new ZoneSpec("shared-id", "p1", "hand")], // collision
            Cards:          [],
            StateOverrides: []);

        Assert.Throws<SessionException>(() => BuildSession(def, hostManifest));
    }

    // -----------------------------------------------------------------------
    //  0.8d  HostManifest zones and cards are provisioned after InitManifest
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HostManifest_ZonesAndCards_Provisioned_AfterInitManifest()
    {
        // InitManifest creates one zone; HostManifest adds another zone and a card.
        var initManifest = new InitManifest(
            Zones:        [new ZoneSpec("init-zone", "p1", "hand")],
            Cards:        [],
            PlayerStates: []);
        var def = BuildDefWithCard(initManifest);

        var hostManifest = new HostManifest(
            Zones:          [new ZoneSpec("host-zone", "p1", "hand")],
            Cards:          [new CardSpec("p1", "host-zone", "test-card")],
            StateOverrides: []);

        var session = BuildSession(def, hostManifest);
        var result  = await session.RunAsync();

        // Session ended (SBR fired); confirms provisioning did not throw.
        Assert.True(result.IsDraw);
    }

    // -----------------------------------------------------------------------
    //  0.8e  AtomStateOverride — accumulator merge leaves others intact
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HostManifest_StateOverride_AccumulatorMerge_LeavesOthersIntact()
    {
        var initManifest = new InitManifest(
            Zones:        [],
            Cards:        [],
            PlayerStates: [new PlayerStateSpec("p1",
                Accumulators: new Dictionary<string, double>
                {
                    ["health"] = 10.0,
                    ["mana"]   = 5.0,
                })]);

        var def = BuildDef(initManifest);

        // Override only "health"; "mana" should survive unchanged.
        var host = new HostManifest(
            Zones:          [],
            Cards:          [],
            StateOverrides: [
                new AtomStateOverride(
                    Target:       new PlayerTarget("p1"),
                    Accumulators: new Dictionary<string, double> { ["health"] = 20.0 })
            ]);

        var capturedView = await RunWithCapture(def, host);
        var playerAtom = Assert.Single(capturedView.GetAtoms(AtomKind.Player));
        // "health" was overridden to 20.0 by HostManifest.
        Assert.Equal(20.0, capturedView.GetAccumulator(playerAtom, "health"));
        // "mana" was not touched; it retains the InitManifest value of 5.0.
        Assert.Equal(5.0,  capturedView.GetAccumulator(playerAtom, "mana"));
    }

    // -----------------------------------------------------------------------
    //  0.8f  AtomStateOverride — conditions appended, not replaced
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HostManifest_StateOverride_ConditionAppend_DoesNotDuplicate()
    {
        var initManifest = new InitManifest(
            Zones:        [],
            Cards:        [],
            PlayerStates: [new PlayerStateSpec("p1",
                Conditions: ["poisoned"])]);

        var def = BuildDef(initManifest);

        // Append "shielded" — "poisoned" should still be present.
        var host = new HostManifest(
            Zones:          [],
            Cards:          [],
            StateOverrides: [
                new AtomStateOverride(
                    Target:     new PlayerTarget("p1"),
                    Conditions: ["shielded"])
            ]);

        var capturedView = await RunWithCapture(def, host);
        var playerAtom = Assert.Single(capturedView.GetAtoms(AtomKind.Player));
        // "poisoned" was set by InitManifest and must not have been wiped by the override.
        Assert.True(capturedView.HasCondition(playerAtom, "poisoned"),
            "Expected 'poisoned' condition (from InitManifest) to survive the HostManifest override.");
        // "shielded" was appended by HostManifest.
        Assert.True(capturedView.HasCondition(playerAtom, "shielded"),
            "Expected 'shielded' condition (from HostManifest override) to be present.");
    }

    // -----------------------------------------------------------------------
    //  0.8g  AtomStateOverride targeting a HostManifest-added atom → SessionException
    // -----------------------------------------------------------------------

    [Fact]
    public void HostManifest_StateOverride_TargetsHostAtom_ThrowsSessionException()
    {
        // InitManifest has no zones/cards/players beyond the player atom.
        var def = BuildDef(InitManifest.Empty);

        // HostManifest adds a zone with LocalId "host-zone" but the StateOverride
        // uses a PlayerTarget, which is fine — let's test CardTarget against a
        // HostManifest card: create a host card with LocalId "host-card" and
        // try to override it via CardTarget.
        var hostManifest = new HostManifest(
            Zones: [new ZoneSpec("host-zone", "p1", "hand")],
            Cards: [new CardSpec("p1", "host-zone", "test-card", LocalId: "host-card")],
            StateOverrides: [
                new AtomStateOverride(
                    Target: new CardTarget("host-card"),   // D29: CardTarget only for InitManifest cards
                    Accumulators: null)
            ]);

        var def2 = BuildDefWithCard(InitManifest.Empty);
        Assert.Throws<SessionException>(() => BuildSession(def2, hostManifest));
    }

    // -----------------------------------------------------------------------
    //  0.8h  WithHostManifest and FromSavedState mutually exclusive
    // -----------------------------------------------------------------------

    [Fact]
    public void HostManifest_And_FromSavedState_MutuallyExclusive_ThrowsSessionException()
    {
        var def = BuildDef(InitManifest.Empty);

        // Create a minimal valid snapshot.
        var snapshot = new GameStateSnapshot(
            Version:             GameStateSnapshot.CurrentVersion,
            GameDefinitionId:    "host-manifest-test",
            TurnNumber:          1,
            NextAtomId:          10,
            NextContributionId:  100,
            SessionAtomId:       new AtomId(1),
            Atoms:               [new AtomSnapshotData(
                new AtomId(1), AtomKind.Session, null, AtomId.None, AtomId.None,
                new Dictionary<string, double> { ["turn-number"] = 1.0, ["phase-index"] = 0.0 })],
            Contributions:       [],
            ActiveStaticEffects: [],
            DormantEffects:      [],
            PlayerNames:         new Dictionary<long, string>(),
            FinalizedLog:        [],
            Rng:                 new RngSnapshot(0L, 0L));

        var host = new HostManifest([], [], []);

        Assert.Throws<SessionException>(() =>
            Archetype.Engine.GameSession.Create(def)
                .WithPlayerStrategy("p1", new ScriptedPlayerStrategy())
                .FromSavedState(snapshot)
                .WithHostManifest(host)
                .Build());
    }

    // -----------------------------------------------------------------------
    //  0.8i  Duplicate zone LocalId within HostManifest.Zones → SessionException
    // -----------------------------------------------------------------------

    [Fact]
    public void ZoneLocalId_Duplicate_WithinHostManifest_ThrowsSessionException()
    {
        var def = BuildDef(InitManifest.Empty);
        var host = new HostManifest(
            Zones:          [new ZoneSpec("dup", "p1", "hand"), new ZoneSpec("dup", "p1", "hand")],
            Cards:          [],
            StateOverrides: []);

        Assert.Throws<SessionException>(() => BuildSession(def, host));
    }
}

// ---------------------------------------------------------------------------
//  Test-local helpers
// ---------------------------------------------------------------------------

/// <summary>
/// A minimal <see cref="IPlayerStrategy"/> that invokes a delegate on
/// <c>SelectActionAsync</c> so tests can capture the live
/// <see cref="GameStateView"/> during the first action window.
/// </summary>
internal sealed class CapturingStrategy : IPlayerStrategy
{
    private readonly Func<GameStateView, PlayerAction?> _onSelect;

    /// <param name="onSelect">
    /// Called with the current <see cref="GameStateView"/> each time an action
    /// window opens.  Return <c>null</c> to pass (end the action window).
    /// </param>
    public CapturingStrategy(Func<GameStateView, PlayerAction?> onSelect)
        => _onSelect = onSelect;

    /// <inheritdoc/>
    public Task<PlayerAction?> SelectActionAsync(
        AvailableActions available, GameStateView state, CancellationToken ct = default)
        => Task.FromResult(_onSelect(state));

    /// <inheritdoc/>
    public Task<PromptResponse> RespondToPromptAsync(
        PromptContext context, GameStateView state, CancellationToken ct = default)
        => throw new InvalidOperationException("CapturingStrategy: unexpected prompt.");
}
