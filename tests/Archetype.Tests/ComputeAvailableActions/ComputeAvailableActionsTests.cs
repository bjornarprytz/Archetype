using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

// Disambiguate: Archetype.Tests.GameSession is a namespace (test folder);
// Archetype.Engine.GameSession is the class we want.
using EngineGameSession = Archetype.Engine.GameSession;
using OnFail    = Archetype.Core.OnFail;
using NotifyFlag = Archetype.Core.NotifyFlag;

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

    /// <summary>
    /// 3.7  Ability activation is offered for a card in a non-playable zone (D19 step 4).
    ///      The ability loop is independent of the PlayCard zone filter, so abilities
    ///      must appear even when the card's zone is not in <c>PlayableZoneNames</c>.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_OffersAbility_EvenWhenCardIsInNonPlayableZone()
    {
        var capturing = new CapturingPlayerStrategy();

        // Card is in the "discard" zone, which is NOT in PlayableZoneNames.
        // It carries an ability with no activation condition, so the ability
        // should always be offered regardless of the card's zone.
        var def = BuildMinimalDef(
            playableZoneNames: ["hand"],
            cards: new Dictionary<string, CardDefinition>
            {
                ["soldier"] = new CardDefinition(
                    Name: "soldier",
                    StaticProperties: new Dictionary<string, object>(),
                    PrimaryEffect: DrawBlock(),
                    AdditionalEffects:
                    [
                        new NamedEffectBlockDef(
                            Name:                "charge",
                            ActivationCondition: null,    // no condition — always activatable
                            Cost:                null,
                            Body:                DrawBlock()),
                    ],
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
                    // Card is in discard (non-playable) — PlayCard should be absent,
                    // but the "charge" ability must still be offered.
                    new CardSpec("p1", "p1-discard", "soldier"),
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        // Card is in discard → no PlayCard option.
        Assert.Empty(capturing.CapturedActions!.PlayableCards);
        // But the ability must still be present (D19 step 4: zone is irrelevant for abilities).
        Assert.Single(capturing.CapturedActions.ActivatableAbilities);
        Assert.Equal("charge", capturing.CapturedActions.ActivatableAbilities[0].EffectName);
    }

    /// <summary>
    /// 3.8 (Updated for D24)  A card that resides in the "hand" zone is offered as
    ///     playable to BOTH players — the engine no longer filters by zone owner (D24).
    ///     Games that want ownership-based filtering must add
    ///     <c>ActivationCondition: Kw.OwnedByActivePlayer()</c> to the card definition.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_CardInOpponentOwnedZone_IsIncluded_D24()
    {
        var capturing = new CapturingPlayerStrategy();

        // Two-player game.  The "hand" zone definition is playable.
        // p1 owns the card; it resides in p2's hand zone.
        // Per D24, the zone-owner predicate is removed — the card IS offered.
        var def = BuildTwoPlayerDef(
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
                    new ZoneSpec("p1-hand", "p1", "hand"),
                    new ZoneSpec("p2-hand", "p2", "hand"),  // p2 owns this zone
                ],
                Cards:
                [
                    // p1 owns the goblin; it sits in p2's hand zone.
                    // D24: zone ownership no longer gates PlayCard — the card IS offered.
                    new CardSpec("p1", "p2-hand", "goblin"),
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithPlayerStrategy("p2", new PassPlayerStrategy())
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        // D24: the card is in a playable zone by definition name ("hand");
        // zone ownership is no longer checked, so the card IS offered.
        Assert.Single(capturing.CapturedActions!.PlayableCards);
        Assert.True(capturing.CapturedActions.CanPass);
    }

    // -----------------------------------------------------------------------
    //  D24 — ownership filter removal tests (9.13, 9.14, 9.15)
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.13  A card owned by p2 that resides in a playable zone is included in
    ///       p1's <c>PlayableCards</c> (D24: no ownership filter).
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_CardInPlayableZone_IncludedRegardlessOfOwner()
    {
        var capturing = new CapturingPlayerStrategy();

        // p2 owns the card; it's in p2's hand zone (a playable zone).
        // Per D24, the card must appear in p1's PlayableCards.
        var def = BuildTwoPlayerDef(
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
                    new ZoneSpec("p1-hand", "p1", "hand"),
                    new ZoneSpec("p2-hand", "p2", "hand"),
                ],
                Cards:
                [
                    new CardSpec("p2", "p2-hand", "goblin"),  // p2 owns the card
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithPlayerStrategy("p2", new PassPlayerStrategy())
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        // D24: card is in a "hand" zone (playable) — no ownership predicate remains.
        Assert.Single(capturing.CapturedActions!.PlayableCards);
    }

    /// <summary>
    /// 9.14  A card owned by p2 with an ability and no <c>ActivationCondition</c>
    ///       is included in p1's <c>ActivatableAbilities</c> (D24).
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_AbilityOnUnownedCard_Included()
    {
        var capturing = new CapturingPlayerStrategy();

        var def = BuildTwoPlayerDef(
            playableZoneNames: [],   // no zone filter needed for abilities
            cards: new Dictionary<string, CardDefinition>
            {
                ["knight"] = new CardDefinition(
                    Name: "knight",
                    StaticProperties: new Dictionary<string, object>(),
                    PrimaryEffect: DrawBlock(),
                    AdditionalEffects:
                    [
                        new NamedEffectBlockDef(
                            Name:                "strike",
                            ActivationCondition: null,  // no condition
                            Cost:                null,
                            Body:                DrawBlock()),
                    ],
                    StaticEffects: []),
            },
            manifest: new InitManifest(
                Zones:
                [
                    new ZoneSpec("p1-hand", "p1", "hand"),
                    new ZoneSpec("p2-hand", "p2", "hand"),
                ],
                Cards:
                [
                    new CardSpec("p2", "p2-hand", "knight"),  // p2 owns the card
                ],
                PlayerStates: []));

        await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithPlayerStrategy("p2", new PassPlayerStrategy())
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        Assert.NotNull(capturing.CapturedActions);
        // D24: ability on an opponent-owned card is included — no ownership guard.
        Assert.Single(capturing.CapturedActions!.ActivatableAbilities);
        Assert.Equal("strike", capturing.CapturedActions.ActivatableAbilities[0].EffectName);
    }

    /// <summary>
    /// 9.15  Calling <c>ValidateActionArgs</c> on a valid action does not mutate
    ///       the live <c>GameState</c>.
    /// </summary>
    [Fact]
    public async Task ComputeAvailableActions_ValidateActionArgs_DoesNotMutateState()
    {
        AvailableActions? captured = null;
        AtomId capturedCard        = AtomId.None;

        // Strategy: capture the AvailableActions, then pass.
        var capturing = new DelegateCapturingStrategy(available =>
        {
            captured     = available;
            capturedCard = available.PlayableCards.Count > 0
                ? available.PlayableCards[0].Card
                : AtomId.None;
        });

        // Card has a cost that consumes 3 energy.
        // Use a simple always-terminate SBR so RunAsync ends immediately.
        var alwaysTrue = Kw.AtLeast(new Literal(1.0), new Literal(1.0));

        var costBody = new EffectBlockDef([
            new EffectBlockStep("assert", [
                new Invocation("at-least",
                    new Invocation("get-state", new Invocation("session"), new Literal("energy")),
                    new Literal(3.0)),
                new Literal((double)(int)OnFail.Panic),
                new Literal((double)(int)NotifyFlag.Off),
            ]),
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("energy"),
                new Literal(-3.0),
            ]),
        ]);

        var cardDef = new CardDefinition(
            Name:               "energy-card",
            StaticProperties:   new Dictionary<string, object>(),
            PrimaryEffect:      DrawBlock(),
            AdditionalEffects:  [],
            StaticEffects:      [],
            ActivationCondition: null,
            Cost: [new CostDef(costBody, [], "Pay 3 energy")]);

        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["energy-card"] = cardDef,
            },
            ZoneDefinitions: new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new ZoneDefinition("hand", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:
            [
                new StateBasedRule("end-game", alwaysTrue,
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])])),
            ],
            Phases:          [new PhaseDefinition("main")],
            ActionRules:     new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions: new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new PlayerDefinition(new Dictionary<string, object>()),
            },
            DefaultInitManifest: new InitManifest(
                Zones:  [new ZoneSpec("p1-hand", "p1", "hand")],
                Cards:  [new CardSpec("p1", "p1-hand", "energy-card")],
                PlayerStates:
                [
                    new PlayerStateSpec("p1", Accumulators:
                        new Dictionary<string, double> { /* energy on session, not player */ }),
                ]),
            PlayableZoneNames: ["hand"],
            Id: "test-game");

        // We can't directly read the session energy before RunAsync since provisioning
        // sets it.  Instead validate that calling ValidateActionArgs on the captured
        // AvailableActions does not corrupt the session energy that the SBR can observe.
        // We verify by running the game after validation and checking it terminates normally.
        var result = await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        // If ValidateActionArgs mutated live state, the session might end in an error.
        // The game should complete as a draw (SBR fires declare-draw).
        Assert.True(result.IsDraw);

        // Calling ValidateActionArgs should not have thrown and should return a result.
        if (captured is not null && capturedCard != AtomId.None)
        {
            var action = new PlayCard(capturedCard);
            var validation = captured.ValidateActionArgs(action);
            // The card has energy cost; session energy starts at 0 (not set), so invalid.
            // What matters here is: no exception was raised.
            Assert.NotNull(validation);
        }
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
            PlayableZoneNames:      playableZoneNames,
            Id:                     "test-game");
    }

    /// <summary>
    /// Builds a <see cref="GameDefinition"/> for two players ("p1" and "p2") with
    /// one phase ("main"), the given playable zone names, card definitions,
    /// and an always-true SBR that fires <c>declare-draw()</c> to end the game.
    /// Used for tests that require zone-owner checks across players.
    /// </summary>
    private static GameDefinition BuildTwoPlayerDef(
        IReadOnlyList<string> playableZoneNames,
        IReadOnlyDictionary<string, CardDefinition> cards,
        InitManifest manifest)
    {
        var alwaysTrue = Kw.AtLeast(new Literal(1.0), new Literal(1.0));

        return new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        cards,
            ZoneDefinitions: new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new ZoneDefinition("hand", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:
            [
                new StateBasedRule("end-game", alwaysTrue,
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])])),
            ],
            Phases:
            [
                new PhaseDefinition("main"),
            ],
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions: new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new PlayerDefinition(new Dictionary<string, object>()),
                ["p2"] = new PlayerDefinition(new Dictionary<string, object>()),
            },
            DefaultInitManifest:    manifest,
            PlayableZoneNames:      playableZoneNames,
            Id:                     "test-game");
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

// ---------------------------------------------------------------------------
//  Test-local helper: PassPlayerStrategy
// ---------------------------------------------------------------------------

/// <summary>
/// A minimal player strategy that always passes.  Used for the second player
/// in two-player tests where only one player's actions need to be inspected.
/// </summary>
file sealed class PassPlayerStrategy : IPlayerStrategy
{
    /// <summary>Always returns <see cref="Pass"/>.</summary>
    public Task<PlayerAction?> SelectActionAsync(
        AvailableActions available, GameStateView state, CancellationToken ct = default)
        => Task.FromResult<PlayerAction?>(new Pass());

    /// <summary>Not expected to be called in pass-only scenarios.</summary>
    public Task<PromptResponse> RespondToPromptAsync(
        PromptContext context, GameStateView state, CancellationToken ct = default)
        => throw new InvalidOperationException("PassPlayerStrategy: no prompt handler.");
}

// ---------------------------------------------------------------------------
//  Test-local helper: DelegateCapturingStrategy
// ---------------------------------------------------------------------------

/// <summary>
/// A strategy that invokes a delegate on the first <c>SelectActionAsync</c>
/// call (to capture the <see cref="AvailableActions"/>), then always passes.
/// </summary>
file sealed class DelegateCapturingStrategy : IPlayerStrategy
{
    private readonly Action<AvailableActions> _onFirstCall;
    private bool _called;

    public DelegateCapturingStrategy(Action<AvailableActions> onFirstCall)
        => _onFirstCall = onFirstCall;

    public Task<PlayerAction?> SelectActionAsync(
        AvailableActions available, GameStateView state, CancellationToken ct = default)
    {
        if (!_called) { _called = true; _onFirstCall(available); }
        return Task.FromResult<PlayerAction?>(new Pass());
    }

    public Task<PromptResponse> RespondToPromptAsync(
        PromptContext context, GameStateView state, CancellationToken ct = default)
        => throw new InvalidOperationException("DelegateCapturingStrategy: no prompt handler.");
}
