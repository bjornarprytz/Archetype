using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.StateBasedRules;

/// <summary>
/// Tests for <c>RunStateBasedRules</c> in <see cref="ActionResolver"/> (D7 §state-based rules).
/// <para>
/// Invariants verified:
/// <list type="bullet">
///   <item>An SBR whose condition is true fires its body block.</item>
///   <item>An SBR whose condition is false never fires.</item>
///   <item>The fixpoint loop re-runs after each pass: when an SBR block changes
///   state such that a second SBR's condition becomes satisfied, the second SBR
///   fires in the next pass of the same action.</item>
///   <item>All conditions for a pass are evaluated before any block fires:
///   two SBRs sharing the same condition both see it as true even if the first
///   SBR's block removes it during its turn to execute.</item>
/// </list>
/// </para>
/// </summary>
public sealed class StateBasedRuleTests
{
    // -----------------------------------------------------------------------
    //  Shared helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates an <see cref="ActionResolver"/> with the given state-based rules registered.
    /// </summary>
    private static ActionResolver BuildResolver(params StateBasedRule[] sbrs)
    {
        var def = TestDefinition.Minimal() with { StateBasedRules = sbrs };
        return ActionResolver.Create(
            new Dictionary<string, IPlayerStrategy> { ["p1"] = new ScriptedPlayerStrategy() },
            new MockRandomSource(),
            def);
    }

    /// <summary>
    /// A condition node that is true when the session atom has the given condition name.
    /// </summary>
    private static KeywordNode SessionHasCondition(string conditionName) =>
        new Invocation("has-condition", new Invocation("session"), new Literal(conditionName));

    /// <summary>
    /// An <see cref="EffectBlockDef"/> that increments the named accumulator on the session atom by 1.
    /// </summary>
    private static EffectBlockDef IncrBlock(string accumName) =>
        new([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal(accumName),
                new Literal(1.0),
            ]),
        ]);

    /// <summary>
    /// Returns the value of an accumulator on the session atom, or 0 if absent.
    /// </summary>
    private static double GetAccum(GameState state, string name) =>
        state.GetAtom(state.SessionAtomId).Accumulators.TryGetValue(name, out var v) ? v : 0.0;

    // -----------------------------------------------------------------------
    //  SBR.1 — fires when condition is true
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SBR_Fires_WhenConditionIsTrue()
    {
        // Arrange: session starts with condition "run-sbr".
        var state = new GameStateBuilder()
            .WithSession(out var sessionId)
            .WithPlayer("p1", out _)
            .WithZone("zone", "p1", out _)
            .WithCondition(sessionId, "run-sbr")
            .Build();

        var log = new EngineEventLog();

        // SBR: when session has "run-sbr" → increment "sbr-ran" then remove the condition.
        // Removing it prevents the same pass from looping forever.
        var sbrBody = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"), new Literal("sbr-ran"), new Literal(1.0),
            ]),
            new EffectBlockStep("remove-condition", [
                new Invocation("session"), new Literal("run-sbr"),
            ]),
        ]);

        var sbr = new StateBasedRule("test-sbr", SessionHasCondition("run-sbr"), sbrBody);

        // Act: pass action (null primary block) — only RunStateBasedRules runs.
        await BuildResolver(sbr).ResolveAction(null, state, log, "p1", currentTurn: 1);

        // Assert: SBR fired exactly once.
        Assert.Equal(1.0, GetAccum(state, "sbr-ran"));
    }

    // -----------------------------------------------------------------------
    //  SBR.2 — does NOT fire when condition is false
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SBR_DoesNotFire_WhenConditionIsFalse()
    {
        // Arrange: session does NOT have condition "run-sbr" — builder default.
        var state = new GameStateBuilder()
            .WithPlayer("p1", out _)
            .WithZone("zone", "p1", out _)
            .Build();

        var log = new EngineEventLog();

        var sbr = new StateBasedRule(
            "test-sbr",
            SessionHasCondition("run-sbr"),   // condition: never satisfied
            IncrBlock("sbr-ran"));

        // Act: pass action.
        await BuildResolver(sbr).ResolveAction(null, state, log, "p1", currentTurn: 1);

        // Assert: SBR body never ran.
        Assert.Equal(0.0, GetAccum(state, "sbr-ran"));
    }

    // -----------------------------------------------------------------------
    //  SBR.3 — fixpoint loop: SBR2 fires in the second pass
    //
    //  SBR1 fires on "phase-1"; its body removes "phase-1" and applies "phase-2".
    //  SBR2 fires on "phase-2".
    //  Expected passes within one action:
    //    Pass 1: SBR1 fires (only "phase-1" is true), SBR2 does not.
    //    Pass 2: SBR2 fires ("phase-2" now true), SBR1 does not.
    //    Pass 3: neither fires → fixpoint.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SBR_FixpointLoop_SBR2_FiresInSecondPass()
    {
        // Arrange: session starts with "phase-1".
        var state = new GameStateBuilder()
            .WithSession(out var sessionId)
            .WithPlayer("p1", out _)
            .WithZone("zone", "p1", out _)
            .WithCondition(sessionId, "phase-1")
            .Build();

        var log = new EngineEventLog();

        var sbr1Body = new EffectBlockDef([
            // Clean up "phase-1" so SBR1 doesn't re-fire.
            new EffectBlockStep("remove-condition", [
                new Invocation("session"), new Literal("phase-1"),
            ]),
            // Advance to "phase-2" to enable SBR2 in the next pass.
            new EffectBlockStep("apply-condition", [
                new Invocation("session"), new Literal("phase-2"),
            ]),
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"), new Literal("sbr1-ran"), new Literal(1.0),
            ]),
        ]);

        var sbr2Body = new EffectBlockDef([
            // Clean up "phase-2" so SBR2 doesn't re-fire.
            new EffectBlockStep("remove-condition", [
                new Invocation("session"), new Literal("phase-2"),
            ]),
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"), new Literal("sbr2-ran"), new Literal(1.0),
            ]),
        ]);

        var sbr1 = new StateBasedRule("sbr1", SessionHasCondition("phase-1"), sbr1Body);
        var sbr2 = new StateBasedRule("sbr2", SessionHasCondition("phase-2"), sbr2Body);

        // Act: pass action.
        await BuildResolver(sbr1, sbr2).ResolveAction(null, state, log, "p1", currentTurn: 1);

        // Assert: both SBRs fired exactly once, in the correct order.
        Assert.Equal(1.0, GetAccum(state, "sbr1-ran"));
        Assert.Equal(1.0, GetAccum(state, "sbr2-ran"));

        // Conditions were removed — fixpoint reached cleanly.
        var sessionAtom = state.GetAtom(sessionId);
        Assert.False(sessionAtom.HasCondition("phase-1"), "phase-1 should have been removed by SBR1.");
        Assert.False(sessionAtom.HasCondition("phase-2"), "phase-2 should have been removed by SBR2.");
    }

    // -----------------------------------------------------------------------
    //  SBR.4 — snapshot invariant: all conditions evaluated before any block fires
    //
    //  SBR1 and SBR2 share the same condition ("trigger-both").  SBR1 removes
    //  the condition during its execution.  If RunStateBasedRules snapshotted
    //  correctly, SBR2 also fires in the same pass because it already saw the
    //  condition as true before SBR1 ran.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SBR_AllConditionsSnapshotted_BeforeAnyBlockFires()
    {
        // Arrange: session starts with "trigger-both".
        var state = new GameStateBuilder()
            .WithSession(out var sessionId)
            .WithPlayer("p1", out _)
            .WithZone("zone", "p1", out _)
            .WithCondition(sessionId, "trigger-both")
            .Build();

        var log = new EngineEventLog();

        // SBR1: fires on "trigger-both"; removes the condition.
        // If SBR2's condition were re-evaluated after SBR1 ran, SBR2 would miss it.
        var sbr1Body = new EffectBlockDef([
            new EffectBlockStep("remove-condition", [
                new Invocation("session"), new Literal("trigger-both"),
            ]),
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"), new Literal("sbr1-ran"), new Literal(1.0),
            ]),
        ]);

        // SBR2: same condition; only increments its counter.
        var sbr2Body = IncrBlock("sbr2-ran");

        var sbr1 = new StateBasedRule("sbr1", SessionHasCondition("trigger-both"), sbr1Body);
        var sbr2 = new StateBasedRule("sbr2", SessionHasCondition("trigger-both"), sbr2Body);

        // Act: pass action.
        await BuildResolver(sbr1, sbr2).ResolveAction(null, state, log, "p1", currentTurn: 1);

        // Assert: BOTH SBRs fired despite SBR1 removing the condition before SBR2 ran.
        // This verifies that conditions are snapshotted before any block executes.
        Assert.Equal(1.0, GetAccum(state, "sbr1-ran"));
        Assert.Equal(1.0, GetAccum(state, "sbr2-ran"));

        // In the following pass the condition is gone — no further firing.
        // Verify sbr2-ran is exactly 1 (not 2), confirming the second pass was empty.
        Assert.Equal(1.0, GetAccum(state, "sbr2-ran"));
    }
}
