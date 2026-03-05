using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

// Disambiguate: Archetype.Tests.GameSession is a namespace (test folder);
// Archetype.Engine.GameSession is the class we want.
using EngineGameSession = Archetype.Engine.GameSession;

namespace Archetype.Tests.ComputeAvailableActions;

/// <summary>
/// Tests for the <c>get-atoms-in-zone</c> built-in primitive and
/// <c>GameSession.ComputeAvailableActions</c> zone-filter + activation-condition
/// contract (D19).
/// <para>
/// Layer 1 (3.1–3.3): isolated state, <see cref="BlockExecutor"/> only.<br/>
/// Layer 3 (3.4–3.6): full <c>GameSession.RunAsync</c> with a
/// capturing player strategy.
/// </para>
/// </summary>
public sealed class ComputeAvailableActionsTests
{
    // -----------------------------------------------------------------------
    //  Layer 1 — get-atoms-in-zone primitive (unit)
    // -----------------------------------------------------------------------

    /// <summary>
    /// 3.1  get-atoms-in-zone returns every atom whose ZoneId matches.
    /// </summary>
    [Fact]
    public void GetAtomsInZone_ReturnsAllAtomsInZone_ForPopulatedZone()
    {
        // Arrange: hand zone with two cards, discard zone with one card
        var state = new GameStateBuilder()
            .WithPlayer("p1",    out _)
            .WithZone("hand",    "p1", out var hand)
            .WithZone("discard", "p1", out var discard)
            .WithCard("a", hand,    "p1", out var ca)
            .WithCard("b", hand,    "p1", out var cb)
            .WithCard("c", discard, "p1", out _)   // in discard — must NOT appear
            .Build();

        var executor = new BlockExecutor();
        var node     = new Invocation("get-atoms-in-zone", new Literal(hand));

        // Act
        var result = executor.EvaluateProperty(node, state) as IReadOnlyList<AtomId>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Contains(ca, result);
        Assert.Contains(cb, result);
    }

    /// <summary>
    /// 3.2  get-atoms-in-zone returns an empty collection when the zone is empty.
    /// </summary>
    [Fact]
    public void GetAtomsInZone_ReturnsEmpty_ForEmptyZone()
    {
        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("hand", "p1", out var hand)
            .Build(); // no cards

        var executor = new BlockExecutor();
        var node     = new Invocation("get-atoms-in-zone", new Literal(hand));

        var result = executor.EvaluateProperty(node, state) as IReadOnlyList<AtomId>;

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    /// <summary>
    /// 3.3  get-atoms-in-zone throws EngineException when argument is not a Zone atom.
    /// </summary>
    [Fact]
    public void GetAtomsInZone_ThrowsEngineException_WhenArgumentIsNotZone()
    {
        var state = new GameStateBuilder()
            .WithPlayer("p1",  out var player)
            .WithZone("hand",  "p1", out var hand)
            .WithCard("card",  hand, "p1", out var card)
            .Build();

        var executor = new BlockExecutor();

        // Passing a Card atom where a Zone is expected
        var node = new Invocation("get-atoms-in-zone", new Literal(card));
        Assert.Throws<EngineException>(() => executor.EvaluateProperty(node, state));

        // Passing a Player atom where a Zone is expected
        var nodePlayer = new Invocation("get-atoms-in-zone", new Literal(player));
        Assert.Throws<EngineException>(() => executor.EvaluateProperty(nodePlayer, state));
    }

    // -----------------------------------------------------------------------
    //  Layer 3 — ComputeAvailableActions integration (via GameSession)
    //
    //  Each test uses a CapturingPlayerStrategy that stores the first
    //  AvailableActions it receives, then returns Pass.  An always-true
    //  SBR fires declare-draw() to terminate the session.
    // -----------------------------------------------------------------------

    /// <summary>
    /// 3.4  Cards not in the playable zone are excluded even if owned by active player.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_ExcludesCards_NotInPlayableZone()
    {
        var capturing = new CapturingPlayerStrategy();

        var def = BuildMinimalDef(
            playableZoneNames: ["hand"],
            cards: new Dictionary<string, CardDefinition>
            {
                ["goblin"] = new CardDefinition(
                    Name: "goblin",
                    StaticProperties: new Dictionary<string, object>(),
                    PrimaryEffect: DrawBlock(),
                    AdditionalEffects: [],
                    StaticEffects: []),
            },
            manifest: new InitManifest(
                Zones:
                [
                    new ZoneSpec("p1-hand",    "p1", "hand"),
                    new ZoneSpec("p1-discard", "p1", "discard"),
                ],
                Cards:
                [
                    new CardSpec("p1", "p1-hand",    "goblin"),   // in hand   → playable
                    new CardSpec("p1", "p1-discard", "goblin"),   // in discard → NOT playable
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        Assert.Single(capturing.CapturedActions!.PlayableCards); // only the hand card
        Assert.True(capturing.CapturedActions.CanPass);
    }

    /// <summary>
    /// 3.5  Cards whose ActivationCondition evaluates to false are not offered.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_ExcludesCards_WithFalseActivationCondition()
    {
        var capturing = new CapturingPlayerStrategy();

        // Condition: health > 0 — card starts with health=0, so condition is false
        var condition = Kw.GreaterThan(
            Kw.GetState(new ParameterRef("source"), new Literal("health")),
            new Literal(0.0));

        var def = BuildMinimalDef(
            playableZoneNames: ["hand"],
            cards: new Dictionary<string, CardDefinition>
            {
                ["vampire"] = new CardDefinition(
                    Name: "vampire",
                    StaticProperties: new Dictionary<string, object>(),
                    PrimaryEffect: DrawBlock(),
                    AdditionalEffects: [],
                    StaticEffects: [],
                    ActivationCondition: condition),
            },
            manifest: new InitManifest(
                Zones:  [new ZoneSpec("p1-hand", "p1", "hand")],
                Cards:  [new CardSpec("p1", "p1-hand", "vampire")],  // health=0 → condition false
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        Assert.Empty(capturing.CapturedActions!.PlayableCards); // condition false → excluded
        Assert.True(capturing.CapturedActions.CanPass);
    }

    /// <summary>
    /// 3.5a  An ActivationCondition that references <c>source</c> evaluates correctly —
    ///       verifies that <c>ComputeAvailableActions</c> injects <c>["source"] = cardId</c>
    ///       before calling <c>EvaluateCondition</c> (architect note, D13).
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_ConditionReferencingSource_IsEvaluatedCorrectly()
    {
        var capturing = new CapturingPlayerStrategy();

        // Condition: source.health >= 10 — card starts with health=10, so condition is true
        var condition = Kw.AtLeast(
            Kw.GetState(new ParameterRef("source"), new Literal("health")),
            new Literal(10.0));

        var def = BuildMinimalDef(
            playableZoneNames: ["hand"],
            cards: new Dictionary<string, CardDefinition>
            {
                ["paladin"] = new CardDefinition(
                    Name: "paladin",
                    StaticProperties: new Dictionary<string, object>(),
                    PrimaryEffect: DrawBlock(),
                    AdditionalEffects: [],
                    StaticEffects: [],
                    ActivationCondition: condition),
            },
            manifest: new InitManifest(
                Zones: [new ZoneSpec("p1-hand", "p1", "hand")],
                Cards:
                [
                    new CardSpec("p1", "p1-hand", "paladin",
                        Accumulators: new Dictionary<string, double> { ["health"] = 10.0 }),
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        // If "source" injection were missing, EvaluateCondition would throw or return false.
        // A non-empty playable list proves the injection happened and the condition resolved.
        Assert.Single(capturing.CapturedActions!.PlayableCards);
    }

    /// <summary>
    /// 3.6  Pass is always present in available actions, even when no cards are playable.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_AlwaysIncludesPass()
    {
        var capturing = new CapturingPlayerStrategy();

        // No cards at all — only Pass should be available
        var def = BuildMinimalDef(
            playableZoneNames: ["hand"],
            cards: new Dictionary<string, CardDefinition>(),
            manifest: new InitManifest(
                Zones:  [new ZoneSpec("p1-hand", "p1", "hand")],
                Cards:  [],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        Assert.Empty(capturing.CapturedActions!.PlayableCards);
        Assert.True(capturing.CapturedActions.CanPass);
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// A dummy primary-effect block: modifies the session accumulator "x" by 0.
    /// Used when a card's effect content doesn't matter for the test.
    /// </summary>
    private static EffectBlockDef DrawBlock() =>
        new([new EffectBlockStep("modify-accumulator", [
            new Invocation("session"),
            new Literal("x"),
            new Literal(0.0),
        ])]);

    /// <summary>
    /// Builds a <see cref="GameDefinition"/> for a single player "p1" with
    /// one phase ("main"), the given playable zone names, card definitions,
    /// and an always-true SBR that fires <c>declare-draw()</c> to end the game.
    /// </summary>
    private static GameDefinition BuildMinimalDef(
        IReadOnlyList<string> playableZoneNames,
        IReadOnlyDictionary<string, CardDefinition> cards,
        InitManifest manifest)
    {
        // SBR condition: always true (1 >= 1)
        var alwaysTrue = Kw.AtLeast(new Literal(1.0), new Literal(1.0));

        return new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        cards,
            ZoneDefinitions: new Dictionary<string, ZoneDefinition>
            {
                ["hand"]    = new ZoneDefinition("hand",    new Dictionary<string, object>()),
                ["discard"] = new ZoneDefinition("discard", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:
            [
                // Terminate after the first action window; prevents infinite loop.
                new StateBasedRule("end-game", alwaysTrue,
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])])),
            ],
            Phases:
            [
                // A phase with no init/cleanup — just opens the action window.
                new PhaseDefinition("main"),
            ],
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions: new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new PlayerDefinition(new Dictionary<string, object>()),
            },
            DefaultInitManifest:    manifest,
            PlayableZoneNames:      playableZoneNames);
    }
}

// ---------------------------------------------------------------------------
//  Test-local helper: CapturingPlayerStrategy
// ---------------------------------------------------------------------------

/// <summary>
/// A player strategy that captures the first <see cref="AvailableActions"/>
/// it receives and then always returns <see cref="Pass"/>.
/// Used to assert on what <c>ComputeAvailableActions</c> offered.
/// </summary>
file sealed class CapturingPlayerStrategy : IPlayerStrategy
{
    /// <summary>The actions offered on the first call to <c>SelectActionAsync</c>.</summary>
    public AvailableActions? CapturedActions { get; private set; }

    public Task<PlayerAction?> SelectActionAsync(
        AvailableActions available, GameStateView state, CancellationToken ct = default)
    {
        CapturedActions ??= available; // capture once
        return Task.FromResult<PlayerAction?>(new Pass());
    }

    public Task<PromptResponse> RespondToPromptAsync(
        PromptContext context, GameStateView state, CancellationToken ct = default)
        => throw new InvalidOperationException("CapturingPlayerStrategy: no prompt handler.");
}
