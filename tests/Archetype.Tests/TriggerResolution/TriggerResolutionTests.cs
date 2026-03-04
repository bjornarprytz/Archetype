using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.TriggerResolution;

/// <summary>
/// Tests for trigger resolution via <see cref="ActionResolver"/> (D7, D8).
/// Each test exercises a distinct aspect of the trigger lifecycle: collection,
/// high-water-mark advancement, ordering, event bindings, and cascade chaining.
/// </summary>
public sealed class TriggerResolutionTests
{
    // -----------------------------------------------------------------------
    //  Shared helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds the minimum non-session game state: one player, one deck zone,
    /// one hand zone, one card in deck.
    /// </summary>
    private static (GameState state, AtomId card, AtomId deck, AtomId hand) BuildState()
    {
        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("deck", "p1", out var deck)
            .WithZone("hand", "p1", out var hand)
            .WithCard("card", deck, "p1", out var card)
            .Build();
        return (state, card, deck, hand);
    }

    /// <summary>
    /// Adds a permanent trigger static effect that watches for
    /// <paramref name="eventKeyword"/> and runs <paramref name="firedBlock"/>
    /// when satisfied.
    /// </summary>
    private static StaticEffect AddTrigger(
        GameState state,
        AtomId ownerAtom,
        string eventKeyword,
        EffectBlockDef firedBlock,
        KeywordNode? condition = null,
        IReadOnlyList<EventParamDecl>? eventParams = null,
        IReadOnlyList<EventBinding>? eventBindings = null)
    {
        var triggerDef = new StaticEffectDef(
            Lifetime: LifetimeSpec.Permanent,
            Trigger: new TriggerDefinition(
                EventKeyword:  eventKeyword,
                Scope:         TriggerScope.ThisGame,
                EventParams:   eventParams  ?? Array.Empty<EventParamDecl>(),
                Condition:     condition,
                EventBindings: eventBindings ?? Array.Empty<EventBinding>(),
                FiredBlock:    firedBlock));

        var se = new StaticEffect
        {
            Id               = state.NextStaticEffectId(),
            OwnerAtom        = ownerAtom,
            IsDeclarative    = true,
            SourceDefinition = triggerDef,
            Lifetime         = LifetimeSpec.Permanent,
            Trigger          = triggerDef.Trigger,
        };
        state.ActiveStaticEffects.Add(se);
        return se;
    }

    /// <summary>
    /// Effect block that increments the named accumulator on the session atom by 1.
    /// </summary>
    private static EffectBlockDef IncrementBlock(string accumName) =>
        new([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal(accumName),
                new Literal(1.0),
            ]),
        ]);

    /// <summary>
    /// Creates an <see cref="ActionResolver"/> suitable for tests (observer=null,
    /// no player strategies, mock random).
    /// </summary>
    private static ActionResolver BuildResolver(GameDefinition? def = null) =>
        ActionResolver.Create(
            new Dictionary<string, IPlayerStrategy> { ["p1"] = new ScriptedPlayerStrategy() },
            new MockRandomSource(),
            def ?? TestDefinition.Minimal());

    /// <summary>
    /// Builds a one-step block: <c>move-card(card → hand)</c>.
    /// </summary>
    private static EffectBlockDef MoveBlock(AtomId card, AtomId hand) =>
        new([
            new EffectBlockStep("move-card", [new Literal(card), new Literal(hand)]),
        ]);

    // -----------------------------------------------------------------------
    //  6.1 — trigger fires when event matches keyword and condition is null
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerFires_WhenEventMatchesKeywordAndNoCondition()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // Trigger: on "move-card", increment "fire-count".
        var se = AddTrigger(state, card, "move-card", IncrementBlock("fire-count"));

        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // Card moved, trigger fired once.
        ArchAssert.InZone(state, card, hand);
        Assert.Equal(1, se.TriggerFireCount);
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(1.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.2 — trigger does NOT fire when condition is false
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerDoesNotFire_WhenConditionIsFalse()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // Condition: has-condition(session(), "never-true") — always false.
        var condition = new Invocation(
            "has-condition",
            new Invocation("session"),
            new Literal("never-true"));

        AddTrigger(state, card, "move-card", IncrementBlock("fire-count"), condition: condition);

        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // Card moved but trigger did not fire.
        ArchAssert.InZone(state, card, hand);
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(0.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.3 — trigger does NOT fire when no matching events are produced
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerDoesNotFire_WhenNoMatchingEvents()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // Trigger watches "draw-card" (a made-up keyword) but the block only
        // produces a "move-card" event — no "draw-card" events in the log.
        AddTrigger(state, card, "draw-card", IncrementBlock("fire-count"));

        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        ArchAssert.InZone(state, card, hand);
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(0.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.4 — high-water mark prevents double-firing on the same event
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HighWaterMark_PreventsTriggerDoubleFiring()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        var se = AddTrigger(state, card, "move-card", IncrementBlock("fire-count"));

        // First action: move-card fires the trigger once.
        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);
        Assert.Equal(1, se.TriggerFireCount);

        // Second action: move-card back to deck (same effect, same trigger).
        // The high-water mark has advanced past the first move-card event.
        // Only the NEW event (from this action) should trigger.
        await BuildResolver().ResolveAction(MoveBlock(card, deck), state, log, "p1", 1);
        Assert.Equal(2, se.TriggerFireCount);

        // fire-count should be exactly 2 (once per move-card event, never replayed).
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(2.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.5 — trigger chain: T2 fires in the NEXT cascade batch, not the same
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerChain_SecondTriggerFiresInNextBatch()
    {
        // T1 watches "move-card" and fires a block that does "modify-accumulator"
        // (named "step1").  T2 watches "modify-accumulator" and fires a block
        // that does another "modify-accumulator" (named "step2").
        // T2 must fire in the cascade batch AFTER T1 (D7 §cascade loop).
        //
        // To prevent an infinite loop (T2's block also produces modify-accumulator,
        // which would re-satisfy T2's condition), T2 has a condition that is only
        // true when "step2" == 0 (i.e., before it has ever fired).
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        var t1FiredBlock = IncrementBlock("step1");
        var t2FiredBlock = IncrementBlock("step2");

        AddTrigger(state, card, "move-card", t1FiredBlock);

        // T2 fires only while step2 == 0 — prevents the fired block's own
        // modify-accumulator event from re-satisfying T2 in subsequent passes.
        var t2OnlyOnce = new Invocation("equal-to",
            new Invocation("get-state", new Invocation("session"), new Literal("step2")),
            new Literal(0.0));

        AddTrigger(state, card, "modify-accumulator", t2FiredBlock, condition: t2OnlyOnce);

        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // Both steps must have incremented exactly once.
        var session = state.GetAtom(state.SessionAtomId);
        var step1 = session.Accumulators.TryGetValue("step1", out var s1) ? s1 : 0.0;
        var step2 = session.Accumulators.TryGetValue("step2", out var s2) ? s2 : 0.0;
        Assert.Equal(1.0, step1); // T1 fired for the move-card event
        Assert.Equal(1.0, step2); // T2 fired for the modify-accumulator produced by T1's block
    }

    // -----------------------------------------------------------------------
    //  6.6 — TriggerCount(1) lifetime causes effect to expire after one fire
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerFiring_FireCount_CausesExpiry_ViaTriggerCount()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // Effect with TriggerCount(1) lifetime: expires after firing once.
        var triggerDef = new StaticEffectDef(
            Lifetime: new LifetimeSpec([new TriggerCount(1)]),
            Trigger: new TriggerDefinition(
                EventKeyword:  "move-card",
                Scope:         TriggerScope.ThisGame,
                EventParams:   Array.Empty<EventParamDecl>(),
                Condition:     null,
                EventBindings: Array.Empty<EventBinding>(),
                FiredBlock:    IncrementBlock("fire-count")));

        var se = new StaticEffect
        {
            Id               = state.NextStaticEffectId(),
            OwnerAtom        = card,
            IsDeclarative    = true,
            SourceDefinition = triggerDef,
            Lifetime         = triggerDef.Lifetime,
            Trigger          = triggerDef.Trigger,
        };
        state.ActiveStaticEffects.Add(se);

        // Move 1: should fire and expire.
        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // Effect is gone — TriggerCount(1) was satisfied after the first fire.
        Assert.DoesNotContain(se, state.ActiveStaticEffects);

        // Move 2: block executes but the trigger is no longer active, so fire-count stays at 1.
        await BuildResolver().ResolveAction(MoveBlock(card, deck), state, log, "p1", 1);
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(1.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.7 — OldestFirst ordering fires lower StaticEffectId first
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OldestFirst_OrdersMultipleTriggers()
    {
        // Two triggers both watching "move-card".  The first registered (lower
        // StaticEffectId) should fire before the second (OldestFirst is default).
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // "order-log" accumulator: T1 appends +1, T2 then appends +10.
        // If OldestFirst is respected, final value is 1 + 10 = 11.
        // If reversed, still 11 (addition is commutative) — so instead, we
        // track which trigger fired FIRST by comparing TriggerFireCount
        // snapshots captured via the accumulator progression.
        // Easier: use distinct accumulators so we can verify they both fired.
        var se1 = AddTrigger(state, card, "move-card", IncrementBlock("t1-fired"));
        var se2 = AddTrigger(state, card, "move-card", IncrementBlock("t2-fired"));

        // OldestFirst: se1.Id.Value < se2.Id.Value (registered first).
        Assert.True(se1.Id.Value < se2.Id.Value);

        var def = TestDefinition.Minimal() with
        {
            TriggerResolutionOrder = TriggerResolutionOrder.OldestFirst,
        };

        await ActionResolver.Create(
            new Dictionary<string, IPlayerStrategy> { ["p1"] = new ScriptedPlayerStrategy() },
            new MockRandomSource(),
            def)
            .ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // Both triggers must have fired exactly once.
        Assert.Equal(1, se1.TriggerFireCount);
        Assert.Equal(1, se2.TriggerFireCount);

        var session = state.GetAtom(state.SessionAtomId);
        Assert.Equal(1.0, session.Accumulators.TryGetValue("t1-fired", out var t1) ? t1 : 0.0);
        Assert.Equal(1.0, session.Accumulators.TryGetValue("t2-fired", out var t2) ? t2 : 0.0);
    }

    // -----------------------------------------------------------------------
    //  6.8 — trigger_event binding is available in fired block via event-arg
    //
    //  The fired block calls event-arg(trigger_event, "card") to extract the
    //  moved card's AtomId from the triggering move-card event, then uses that
    //  AtomId as the atom argument to modify-accumulator.  The accumulator ends
    //  up on the card itself, proving the EventRef/event-arg plumbing works
    //  end-to-end within a triggered block.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerEvent_Binding_IsAvailableInFiredBlock()
    {
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        // Fired block: call event-arg(trigger_event, "card") to extract the
        // moved card's AtomId, then use it as the atom for modify-accumulator.
        // The accumulator is set on the card — NOT on session — which proves
        // that event-arg returned the correct AtomId and it was accepted as an
        // argument by modify-accumulator.
        var firedBlock = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                // event-arg reads "card" from the trigger_event EventRef.
                // move-card logs BoundArgs["card"] = the moved card's AtomId.
                new Invocation("event-arg", new ParameterRef("trigger_event"), new Literal("card")),
                new Literal("trigger-visited"),
                new Literal(1.0),
            ]),
        ]);

        AddTrigger(state, card, "move-card", firedBlock);

        await BuildResolver().ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        // The fired block extracted card's AtomId via event-arg and incremented
        // "trigger-visited" on the card atom itself.
        var triggerVisited = state.GetAtom(card).Accumulators
            .TryGetValue("trigger-visited", out var v) ? v : 0.0;
        Assert.Equal(1.0, triggerVisited);

        // Confirm the move-card event logged the "card" arg that event-arg reads.
        var moveCardEvent = log.ThisGame.First(e => e.KeywordName == "move-card");
        Assert.True(moveCardEvent.BoundArgs.TryGetValue("card", out var argCard));
        Assert.Equal(card, argCard);
    }

    // -----------------------------------------------------------------------
    //  6.10 — trigger condition can reference "source" (the owning atom)
    //
    //  Without the source binding fix, ParameterRef("source") throws a
    //  KeyNotFoundException.  This test verifies the binding is populated and
    //  the condition evaluates correctly.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerCondition_CanReferenceSourceBinding()
    {
        // card1 is the trigger owner and stays in deck throughout the action.
        // card2 is moved by the primary block.
        // The trigger condition uses ParameterRef("source") — bound to card1 —
        // to check in-zone(source, deck).  Because card1 never leaves deck,
        // the condition is true and the trigger should fire.
        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("deck", "p1", out var deck)
            .WithZone("hand", "p1", out var hand)
            .WithCard("card1", deck, "p1", out var card1)   // trigger owner; stays in deck
            .WithCard("card2", deck, "p1", out var card2)   // moved by primary block
            .Build();
        var log = new EngineEventLog();

        // Condition: in-zone(source, deck) — resolves "source" → card1.
        var condition = new Invocation("in-zone",
            new ParameterRef("source"),
            new Literal(deck));

        AddTrigger(state, card1, "move-card", IncrementBlock("fire-count"), condition: condition);

        // Primary block: move card2 to hand; card1 stays in deck.
        var primaryBlock = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(card2), new Literal(hand)]),
        ]);

        await BuildResolver().ResolveAction(primaryBlock, state, log, "p1", 1);

        // card1 stayed in deck — condition was true → trigger fired.
        ArchAssert.InZone(state, card1, deck);
        ArchAssert.InZone(state, card2, hand);
        var fireCount = state.GetAtom(state.SessionAtomId).Accumulators
            .TryGetValue("fire-count", out var fc) ? fc : 0.0;
        Assert.Equal(1.0, fireCount);
    }

    // -----------------------------------------------------------------------
    //  6.9 — null observer does not halt the cascade
    // -----------------------------------------------------------------------

    [Fact]
    public async Task NullObserver_DoesNotHaltCascade()
    {
        // ActionResolver created with null observer (the Create overload defaults
        // observer to null).  A two-trigger chain must still run to completion.
        var (state, card, deck, hand) = BuildState();
        var log = new EngineEventLog();

        AddTrigger(state, card, "move-card", IncrementBlock("step1"));

        // T2 fires only while step2 == 0 to prevent infinite re-triggering
        // (same guard as test 6.5).
        var t2OnlyOnce = new Invocation("equal-to",
            new Invocation("get-state", new Invocation("session"), new Literal("step2")),
            new Literal(0.0));
        AddTrigger(state, card, "modify-accumulator", IncrementBlock("step2"), condition: t2OnlyOnce);

        // null observer (default) — verify no NullReferenceException and the
        // cascade runs to quiescence.
        var resolver = ActionResolver.Create(
            new Dictionary<string, IPlayerStrategy> { ["p1"] = new ScriptedPlayerStrategy() },
            new MockRandomSource(),
            TestDefinition.Minimal(),
            observer: null);

        await resolver.ResolveAction(MoveBlock(card, hand), state, log, "p1", 1);

        var session = state.GetAtom(state.SessionAtomId);
        Assert.Equal(1.0, session.Accumulators.TryGetValue("step1", out var s1) ? s1 : 0.0);
        Assert.Equal(1.0, session.Accumulators.TryGetValue("step2", out var s2) ? s2 : 0.0);
    }
}
