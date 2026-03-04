using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.MoveCard;

/// <summary>
/// Layer 1 unit tests for the <c>move-card</c> primitive.
/// Each test constructs only the state it needs and verifies a single
/// invariant (D16 §Layer 1).
/// </summary>
public sealed class MoveCardLayer1Tests
{
    // -----------------------------------------------------------------------
    //  Shared setup helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a minimal state with one player, two zones (origin and dest),
    /// and one card initially in origin.
    /// </summary>
    private static (GameState state, AtomId player, AtomId origin, AtomId dest, AtomId card)
        BuildBasicState()
    {
        var state = new GameStateBuilder()
            .WithPlayer("player1", out var player)
            .WithZone("origin", "player1", out var origin)
            .WithZone("dest",   "player1", out var dest)
            .WithCard("goblin", origin, "player1", out var card)
            .Build();

        return (state, player, origin, dest, card);
    }

    private static (BlockExecutor executor, EngineEventLog log, Archetype.Engine.ExecutionContext ctx)
        BuildExecutor(GameState state)
    {
        var executor = new BlockExecutor();
        var log      = new EngineEventLog();
        var ctx      = TestContext.Create(state, log);
        return (executor, log, ctx);
    }

    // -----------------------------------------------------------------------
    //  6.1 — move-card updates AtomSnapshot.ZoneId to the destination zone
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MoveCard_UpdatesCardZoneId_ToDestination()
    {
        // Arrange
        var (state, _, origin, dest, card) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        Assert.Equal(origin, state.GetAtom(card).ZoneId); // pre-condition

        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new ParameterRef("card"), new ParameterRef("dest")]),
        ]);
        ctx.Bindings["card"] = card;
        ctx.Bindings["dest"] = dest;

        // Act
        await executor.ExecuteBlock(block, ctx);

        // Assert — card is now in dest, not in origin.
        ArchAssert.InZone(state, card, dest);
        ArchAssert.NotInZone(state, card, origin);
    }

    // -----------------------------------------------------------------------
    //  6.2 — move-card logs correct event; origin reflects pre-move zone
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MoveCard_LogsEvent_WithCorrectCardOriginAndDestination()
    {
        // Arrange
        var (state, _, origin, dest, card) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new ParameterRef("card"), new ParameterRef("dest")]),
        ]);
        ctx.Bindings["card"] = card;
        ctx.Bindings["dest"] = dest;

        // Act
        await executor.ExecuteBlock(block, ctx);

        // Assert — event logged with bound args including pre-move origin.
        var ev = ArchAssert.EventLogged(
            log.ThisGame,
            "move-card",
            ("card",        (object)card),
            ("origin",      (object)origin), // captured before mutation
            ("destination", (object)dest));

        Assert.True(ev.SequenceNumber > 0, "Event must have been assigned a sequence number.");
    }

    [Fact]
    public async Task MoveCard_OriginReflectsZoneAtCallTime_NotAfterSubsequentMove()
    {
        // Arrange — move the card twice; each event captures origin at call time.
        var state = new GameStateBuilder()
            .WithPlayer("player1", out _)
            .WithZone("zone-a", "player1", out var zoneA)
            .WithZone("zone-b", "player1", out var zoneB)
            .WithZone("zone-c", "player1", out var zoneC)
            .WithCard("card",   zoneA, "player1", out var card)
            .Build();

        var (executor, log, ctx) = BuildExecutor(state);

        // Two move steps in one block: A→B then B→C.
        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new ParameterRef("card"), new ParameterRef("zoneB")]),
            new EffectBlockStep("move-card", [new ParameterRef("card"), new ParameterRef("zoneC")]),
        ]);
        ctx.Bindings["card"]  = card;
        ctx.Bindings["zoneB"] = zoneB;
        ctx.Bindings["zoneC"] = zoneC;

        // Act
        await executor.ExecuteBlock(block, ctx);

        // Assert — both events logged; each origin captures its own call-time zone.
        var events = log.ThisGame
            .Where(e => e.KeywordName == "move-card")
            .OrderBy(e => e.SequenceNumber)
            .ToList();

        Assert.Equal(2, events.Count);

        Assert.Equal(zoneA, events[0].BoundArgs["origin"]);      // first move: A→B
        Assert.Equal(zoneB, events[0].BoundArgs["destination"]);

        Assert.Equal(zoneB, events[1].BoundArgs["origin"]);      // second move: B→C
        Assert.Equal(zoneC, events[1].BoundArgs["destination"]);
    }

    // -----------------------------------------------------------------------
    //  6.3 — self-move completes without error and still logs an event
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MoveCard_SelfMove_CompletesWithoutError_AndLogsEvent()
    {
        // Arrange
        var (state, _, origin, _, card) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        var block = new EffectBlockDef([
            // Destination is the same as origin.
            new EffectBlockStep("move-card", [new ParameterRef("card"), new ParameterRef("origin")]),
        ]);
        ctx.Bindings["card"]   = card;
        ctx.Bindings["origin"] = origin;

        // Act — must not throw.
        await executor.ExecuteBlock(block, ctx);

        // Assert — card still in origin; event logged with origin == destination.
        ArchAssert.InZone(state, card, origin);

        var ev = ArchAssert.EventLogged(
            log.ThisGame,
            "move-card",
            ("card",        (object)card),
            ("origin",      (object)origin),
            ("destination", (object)origin)); // origin == destination

        Assert.Equal(origin, ev.BoundArgs["origin"]);
        Assert.Equal(origin, ev.BoundArgs["destination"]);
    }

    // -----------------------------------------------------------------------
    //  6.4 — invalid destination raises EngineException
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MoveCard_InvalidDestination_NonExistentAtom_ThrowsEngineException()
    {
        // Arrange — destination atom does not exist in state.
        var (state, _, origin, _, card) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        var nonExistent = new AtomId(99999); // definitely not in state

        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(card), new Literal(nonExistent)]),
        ]);

        // Act & Assert.
        await Assert.ThrowsAsync<EngineException>(async () =>
            await executor.ExecuteBlock(block, ctx));

        // Card must not have moved — state unchanged on failure.
        ArchAssert.InZone(state, card, origin);
    }

    [Fact]
    public async Task MoveCard_CardArgIsZone_ThrowsEngineException()
    {
        // Arrange — pass a zone atom as the 'card' argument (wrong kind).
        var (state, _, origin, dest, _) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        // origin is a Zone atom — should be rejected as the 'card' parameter.
        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(origin), new Literal(dest)]),
        ]);

        // Act & Assert — wrong kind for 'card' must throw.
        await Assert.ThrowsAsync<EngineException>(async () =>
            await executor.ExecuteBlock(block, ctx));
    }

    [Fact]
    public async Task MoveCard_DestinationIsCard_NotZone_ThrowsEngineException()
    {
        // Arrange — pass a card atom as destination (wrong kind).
        var (state, _, origin, _, card) = BuildBasicState();
        var (executor, log, ctx) = BuildExecutor(state);

        // The card itself is used as the "destination" — wrong atom kind.
        var block = new EffectBlockDef([
            new EffectBlockStep("move-card", [new Literal(card), new Literal(card)]),
        ]);

        await Assert.ThrowsAsync<EngineException>(async () =>
            await executor.ExecuteBlock(block, ctx));
    }
}
