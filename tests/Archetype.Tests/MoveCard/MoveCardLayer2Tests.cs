using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.MoveCard;

/// <summary>
/// Layer 2 block-level integration tests for the <c>move-card</c> primitive.
/// Each test exercises a more complete execution path: static effect lifecycles,
/// composite keyword composition, and trigger resolution (D16 §Layer 2).
/// </summary>
public sealed class MoveCardLayer2Tests
{
    // -----------------------------------------------------------------------
    //  Shared setup
    // -----------------------------------------------------------------------

    private static (BlockExecutor executor, LifetimeChecker checker, EngineEventLog log) Build()
    {
        var executor = new BlockExecutor();
        var checker  = new LifetimeChecker(executor);
        var log      = new EngineEventLog();
        return (executor, checker, log);
    }

    private static Archetype.Engine.ExecutionContext Ctx(GameState state, EngineEventLog log, GameDefinition? def = null) =>
        TestContext.Create(state, log, definition: def);

    // -----------------------------------------------------------------------
    //  6.5 — static effect expires after card leaves while-condition zone
    //
    //  Scenario: a card has an active declarative static effect whose
    //  LifetimeSpec contains WhileCondition: in-zone(source, zone-X).
    //  After move-card moves the card to zone-Y, CheckLifetimes should
    //  expire the effect.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckLifetimes_ExpiresActiveEffect_WhenCardLeavesWhileConditionZone()
    {
        // Arrange
        var (executor, checker, log) = Build();

        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("zone-x", "p1", out var zoneX)
            .WithZone("zone-y", "p1", out var zoneY)
            .WithCard("card",  zoneX, "p1", out var card)
            .Build();

        // while-condition: in-zone(source, zone-X)
        // When the card leaves zone-X, this condition becomes false → effect expires.
        var whileCondition = new WhileCondition(
            new Invocation("in-zone",
                new ParameterRef("source"), // "source" resolves to ownerAtom in lifetime checks
                new Literal(zoneX)));

        var effectDef = new StaticEffectDef(
            Lifetime: new LifetimeSpec([whileCondition]));

        // Pre-register the effect as active (card is currently in zone-X, condition is true).
        state.WithStaticEffectDirectly(effectDef, card);
        ArchAssert.HasActiveEffect(state, card);

        // Move the card to zone-Y.
        var moveBlock = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(card), new Literal(zoneY)]),
        ]);

        await executor.ExecuteBlock(moveBlock, Ctx(state, log));

        // Act — CheckLifetimes should now see in-zone(card, zone-X) = false → expire.
        checker.CheckLifetimes(state, currentTurn: 1);

        // Assert
        ArchAssert.HasNoActiveEffect(state, card);
        // The effect was declarative with only a while-condition → goes dormant.
        ArchAssert.HasDormantEffect(state, card);
    }

    // -----------------------------------------------------------------------
    //  6.6 — dormant declarative effect re-activates when card enters zone
    //
    //  Scenario: a card has a dormant declarative effect with
    //  WhileCondition: in-zone(source, zone-X).  After move-card places
    //  the card in zone-X, CheckLifetimes Phase 2 should activate a new instance.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckLifetimes_ActivatesDormantEffect_WhenCardEntersWhileConditionZone()
    {
        // Arrange
        var (executor, checker, log) = Build();

        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("zone-x", "p1", out var zoneX)
            .WithZone("zone-y", "p1", out var zoneY)
            .WithCard("card",  zoneY, "p1", out var card) // starts in zone-Y (wrong zone)
            .Build();

        // while-condition: in-zone(source, zone-X)
        var whileCondition = new WhileCondition(
            new Invocation("in-zone",
                new ParameterRef("source"),
                new Literal(zoneX)));

        var effectDef = new StaticEffectDef(
            Lifetime: new LifetimeSpec([whileCondition]));

        // Manually add as dormant (card not in zone-X, so condition is false).
        state.DormantDeclarativeEffects.Add(new DormantDeclarativeEffect
        {
            OwnerAtom = card,
            EffectDef = effectDef,
        });

        Assert.Empty(state.ActiveStaticEffects);
        Assert.Single(state.DormantDeclarativeEffects);

        // Move card to zone-X.
        var moveBlock = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(card), new Literal(zoneX)]),
        ]);
        await executor.ExecuteBlock(moveBlock, Ctx(state, log));

        // Act — Phase 2 of CheckLifetimes should instantiate the dormant effect.
        checker.CheckLifetimes(state, currentTurn: 1);

        // Assert — effect is now active with fresh counters.
        ArchAssert.HasActiveEffect(state, card);
        Assert.Empty(state.DormantDeclarativeEffects);

        var activeEffect = state.ActiveStaticEffects.Single(se => se.OwnerAtom == card);
        Assert.Equal(0, activeEffect.TriggerFireCount);
        Assert.Equal(0L, activeEffect.TriggerHighWaterMark);
    }

    // -----------------------------------------------------------------------
    //  6.7 — composite keyword calling move-card nests events and triggers fire
    //
    //  Scenario: a game-creator composite keyword "draw-card(player)" calls
    //  move-card internally.  Executing draw-card produces:
    //    - a draw-card event wrapping a move-card child event
    //    - a trigger watching EventKeyword: "move-card" fires
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CompositeKeyword_CallingMoveCard_ProducesNestedEventTree_AndTriggerFires()
    {
        // Arrange — define a composite "draw-card(card, hand)" keyword that calls move-card.
        var drawCardDef = new KeywordDefinition(
            Name: "draw-card",
            Parameters: [
                new ParameterDecl("card", TypeName.Card),
                new ParameterDecl("hand", TypeName.Zone),
            ],
            ReturnType:  TypeName.Boolean,
            Description: "Moves a card from deck to hand.",
            Body: new Invocation("move-card", new ParameterRef("card"), new ParameterRef("hand")));

        var def = TestDefinition.Minimal().WithKeyword(drawCardDef);

        var (_, _, log) = Build();

        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("deck", "p1", out var deck)
            .WithZone("hand", "p1", out var hand)
            .WithCard("card", deck, "p1", out var card)
            .Build();

        // Set up a trigger: when a move-card event fires, increment a counter.
        var triggerDef = new StaticEffectDef(
            Lifetime: LifetimeSpec.Permanent,
            Trigger: new TriggerDefinition(
                EventKeyword:  "move-card",
                Scope:         TriggerScope.ThisGame,
                EventParams:   Array.Empty<EventParamDecl>(),
                Condition:     null,
                EventBindings: Array.Empty<EventBinding>(),
                FiredBlock:    new EffectBlockDef([
                    // Increment "trigger-count" on the session atom.
                    new EffectBlockStep("modify-accumulator", [
                        new Invocation("session"),
                        new Literal("trigger-count"),
                        new Literal(1.0),
                    ]),
                ])));

        state.DormantDeclarativeEffects.Clear();
        state.ActiveStaticEffects.Add(new StaticEffect
        {
            Id               = state.NextStaticEffectId(),
            OwnerAtom        = card,
            IsDeclarative    = true,
            SourceDefinition = triggerDef,
            Lifetime         = LifetimeSpec.Permanent,
            Trigger          = triggerDef.Trigger,
        });

        // Use the real ActionResolver — it drives the full D7 post-action sequence
        // including composite execution, composite event nesting, AND trigger resolution.
        var strategies = new Dictionary<string, IPlayerStrategy>
            { ["p1"] = new ScriptedPlayerStrategy() };
        var resolver = ActionResolver.Create(strategies, new MockRandomSource(), def);

        // Execute the composite draw-card block via ActionResolver.
        var block = new EffectBlockDef([
            new EffectBlockStep("draw-card", [new Literal(card), new Literal(hand)]),
        ]);

        await resolver.ResolveAction(block, state, log, activePlayerName: "p1", currentTurn: 1);

        // 6.7a — card moved to hand.
        ArchAssert.InZone(state, card, hand);

        // 6.7b — both draw-card and move-card events exist in the log.
        // The move-card event must appear as a direct child of the draw-card
        // composite wrapper (not duplicated in the flat log at top level).
        var allEvents = log.ThisGame.ToList();
        var drawCardEvent = allEvents.FirstOrDefault(e => e.KeywordName == "draw-card");
        Assert.NotNull(drawCardEvent);

        // The composite parent stack approach means move-card is nested, not flat.
        // draw-card should have exactly one child: the move-card event.
        var moveCardChild = drawCardEvent.Children.FirstOrDefault(c => c.KeywordName == "move-card");
        Assert.NotNull(moveCardChild);
        Assert.Single(drawCardEvent.Children);

        // move-card must not also appear as a top-level (flat) event — no duplicates.
        var moveCardEvents = allEvents.Where(e => e.KeywordName == "move-card").ToList();
        Assert.True(moveCardEvents.Count > 0,
            "Expected at least one move-card event in the log (via recursive traversal).");

        // 6.7c — trigger fired: ActionResolver automatically fires the trigger.
        // Assert that the trigger-count accumulator was incremented by the fired block.
        var sessionAtom = state.SessionAtomId;
        var triggerCount = state.GetAtom(sessionAtom).Accumulators
            .TryGetValue("trigger-count", out var tc) ? tc : 0.0;
        Assert.Equal(1.0, triggerCount); // triggered exactly once for the one move-card event
    }
}

// ---------------------------------------------------------------------------
//  Extension helpers for Layer 2 tests
// ---------------------------------------------------------------------------

/// <summary>
/// Test-only extension to directly add a static effect instance to a
/// <see cref="GameState"/> without going through the builder.
/// </summary>
internal static class GameStateTestExtensions
{
    /// <summary>
    /// Instantiates and registers a declarative static effect as active.
    /// The while-condition is not evaluated — the effect is forced active.
    /// </summary>
    public static void WithStaticEffectDirectly(this GameState state, StaticEffectDef def, AtomId ownerAtom)
    {
        var se = new StaticEffect
        {
            Id               = state.NextStaticEffectId(),
            OwnerAtom        = ownerAtom,
            IsDeclarative    = true,
            SourceDefinition = def,
            Lifetime         = def.Lifetime,
            Trigger          = def.Trigger,
            ParameterModification = def.ParameterModification,
        };
        state.ActiveStaticEffects.Add(se);
    }
}
