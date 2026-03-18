using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

// Disambiguate engine internal types.
using EngineContext  = Archetype.Engine.ExecutionContext;
using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.BuiltIns;

/// <summary>
/// Unit tests for the <c>assert</c> built-in keyword (D20).
/// <para>
/// Tests cover:
/// <list type="bullet">
///   <item>9.1 — outside cost body, <c>continue/on</c>: OnDiagnostic called, no exception, execution continues</item>
///   <item>9.2 — outside cost body, <c>stop/on</c>: OnDiagnostic called, block halts, no exception</item>
///   <item>9.3 — outside cost body, <c>panic/on</c>: OnDiagnostic called before EngineException</item>
///   <item>9.4 — outside cost body, <c>panic/off</c>: OnDiagnostic NOT called, EngineException raised</item>
///   <item>9.5 — inside cost body: always panics silently regardless of args</item>
///   <item>9.6 — assert never appends to EventLog under any outcome</item>
/// </list>
/// </para>
/// </summary>
public sealed class AssertTests
{
    // -----------------------------------------------------------------------
    //  Test infrastructure helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// A minimal <see cref="IEngineObserver"/> that records every
    /// <see cref="DiagnosticEvent"/> delivered via <see cref="OnDiagnostic"/>.
    /// </summary>
    private sealed class RecordingObserver : IEngineObserver
    {
        public List<DiagnosticEvent> Diagnostics { get; } = new();

        public Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)
            => Task.CompletedTask;

        public Task<CascadeDirective> OnTriggerCascadeAsync(int iterationCount)
            => Task.FromResult(CascadeDirective.Continue);

        public void OnDiagnostic(DiagnosticEvent e)
            => Diagnostics.Add(e);
    }

    /// <summary>
    /// Builds a minimal execution context with the given observer and a
    /// fresh event log already in an open-action scope.
    /// </summary>
    private static (EngineContext ctx, EngineEventLog log, GameState state) BuildCtx(
        IEngineObserver? observer = null)
    {
        var state = new GameStateBuilder().Build();
        var log   = new EngineEventLog();
        log.OpenAction();

        var ctx = new EngineContext(
            gameState:        state,
            eventLog:         log,
            bindings:         new Dictionary<string, object>(),
            strategies:       new Dictionary<string, IPlayerStrategy>
            {
                ["p1"] = new ScriptedPlayerStrategy(),
            },
            randomSource:     new MockRandomSource(),
            definition:       TestDefinition.Minimal(),
            activePlayerName: "p1",
            observer:         observer);

        return (ctx, log, state);
    }

    /// <summary>
    /// Builds a one-step block that calls <c>assert(condition, on_fail, notify)</c>.
    /// </summary>
    private static EffectBlockDef AssertBlock(bool condition, OnFail onFail, NotifyFlag notify) =>
        new([new EffectBlockStep("assert", [
            new Literal(condition),
            new Literal((double)(int)onFail),
            new Literal((double)(int)notify),
        ])]);

    /// <summary>
    /// Builds a two-step block that first calls <c>assert(…)</c> and then
    /// calls a sentinel <c>modify-accumulator</c> so we can detect whether
    /// execution continued past the assert.
    /// </summary>
    private static EffectBlockDef AssertThenModifyBlock(
        bool condition, OnFail onFail, NotifyFlag notify, AtomId sessionAtom) =>
        new([
            new EffectBlockStep("assert", [
                new Literal(condition),
                new Literal((double)(int)onFail),
                new Literal((double)(int)notify),
            ]),
            // Sentinel step: executed only if execution continues past assert.
            new EffectBlockStep("modify-accumulator", [
                new Literal(sessionAtom),
                new Literal("sentinel"),
                new Literal(1.0),
            ]),
        ]);

    // -----------------------------------------------------------------------
    //  9.1 — continue/on: OnDiagnostic called, no exception, execution continues
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.1  assert(false, continue, on) outside a cost body:
    /// <see cref="IEngineObserver.OnDiagnostic"/> is called once; no exception is raised;
    /// execution continues to the next step.
    /// </summary>
    [Fact]
    public async Task Assert_OutsideCostBody_ContinueNotify_CallsOnDiagnosticNoException()
    {
        var observer = new RecordingObserver();
        var (ctx, _, state) = BuildCtx(observer);
        var executor = new BlockExecutor();

        var block = AssertThenModifyBlock(
            condition: false,
            onFail:    OnFail.Continue,
            notify:    NotifyFlag.On,
            sessionAtom: state.SessionAtomId);

        // Must not throw.
        await executor.ExecuteBlock(block, ctx);

        // OnDiagnostic must have been called exactly once.
        Assert.Single(observer.Diagnostics);
        Assert.Equal(DiagnosticKind.AssertionFailed, observer.Diagnostics[0].Kind);
        Assert.Equal(OnFail.Continue, observer.Diagnostics[0].OnFail);

        // Execution continued — sentinel step must have fired.
        var sentinel = state.GetAtom(state.SessionAtomId)
            .Accumulators.TryGetValue("sentinel", out var v) ? v : 0.0;
        Assert.Equal(1.0, sentinel);
    }

    // -----------------------------------------------------------------------
    //  9.2 — stop/on: OnDiagnostic called, block halts, no exception
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.2  assert(false, stop, on) outside a cost body:
    /// <see cref="IEngineObserver.OnDiagnostic"/> is called; the block halts
    /// cleanly (no exception); steps after <c>assert</c> are not executed.
    /// </summary>
    [Fact]
    public async Task Assert_OutsideCostBody_StopNotify_CallsOnDiagnosticHaltsBlock()
    {
        var observer = new RecordingObserver();
        var (ctx, _, state) = BuildCtx(observer);
        var executor = new BlockExecutor();

        var block = AssertThenModifyBlock(
            condition: false,
            onFail:    OnFail.Stop,
            notify:    NotifyFlag.On,
            sessionAtom: state.SessionAtomId);

        // Must not throw (block halts cleanly).
        await executor.ExecuteBlock(block, ctx);

        // OnDiagnostic must have been called.
        Assert.Single(observer.Diagnostics);
        Assert.Equal(OnFail.Stop, observer.Diagnostics[0].OnFail);

        // Block halted — sentinel step must NOT have fired.
        var sentinel = state.GetAtom(state.SessionAtomId)
            .Accumulators.TryGetValue("sentinel", out var v) ? v : 0.0;
        Assert.Equal(0.0, sentinel);
    }

    // -----------------------------------------------------------------------
    //  9.3 — panic/on: OnDiagnostic called BEFORE EngineException
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.3  assert(false, panic, on) outside a cost body:
    /// <see cref="IEngineObserver.OnDiagnostic"/> is called before
    /// <see cref="EngineException"/> is raised.
    /// </summary>
    [Fact]
    public async Task Assert_OutsideCostBody_PanicNotify_CallsOnDiagnosticThenThrows()
    {
        var observer = new RecordingObserver();
        var (ctx, _, _) = BuildCtx(observer);
        var executor = new BlockExecutor();

        var block = AssertBlock(condition: false, onFail: OnFail.Panic, notify: NotifyFlag.On);

        // Must throw EngineException.
        await Assert.ThrowsAsync<EngineException>(() =>
            executor.ExecuteBlock(block, ctx));

        // OnDiagnostic must have been called (before the exception).
        Assert.Single(observer.Diagnostics);
        Assert.Equal(DiagnosticKind.AssertionFailed, observer.Diagnostics[0].Kind);
        Assert.Equal(OnFail.Panic, observer.Diagnostics[0].OnFail);
    }

    // -----------------------------------------------------------------------
    //  9.4 — panic/off: OnDiagnostic NOT called, EngineException raised
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.4  assert(false, panic, off) outside a cost body:
    /// <see cref="IEngineObserver.OnDiagnostic"/> is NOT called;
    /// <see cref="EngineException"/> is raised.
    /// </summary>
    [Fact]
    public async Task Assert_OutsideCostBody_PanicNoNotify_NoOnDiagnosticThrows()
    {
        var observer = new RecordingObserver();
        var (ctx, _, _) = BuildCtx(observer);
        var executor = new BlockExecutor();

        var block = AssertBlock(condition: false, onFail: OnFail.Panic, notify: NotifyFlag.Off);

        // Must throw.
        await Assert.ThrowsAsync<EngineException>(() =>
            executor.ExecuteBlock(block, ctx));

        // OnDiagnostic must NOT have been called.
        Assert.Empty(observer.Diagnostics);
    }

    // -----------------------------------------------------------------------
    //  9.5 — inside cost body: always panics silently regardless of args
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.5  assert(false, continue, on) inside a cost body (<c>IsCostBody = true</c>):
    /// <see cref="EngineException"/> is raised; <c>OnDiagnostic</c> is NOT called.
    /// The call-site arguments are ignored.
    /// </summary>
    [Fact]
    public async Task Assert_InsideCostBody_AlwaysPanicsNoNotify_RegardlessOfArguments()
    {
        var observer = new RecordingObserver();
        var (ctx, _, _) = BuildCtx(observer);
        ctx.IsCostBody = true;  // simulate cost-body execution mode

        var executor = new BlockExecutor();

        // Use continue/on — the most permissive args outside a cost body.
        // Inside cost body these should be overridden to panic/off.
        var block = AssertBlock(condition: false, onFail: OnFail.Continue, notify: NotifyFlag.On);

        // Must raise EngineException (panic), not halt or continue.
        await Assert.ThrowsAsync<EngineException>(() =>
            executor.ExecuteBlock(block, ctx));

        // OnDiagnostic must NOT be called even though notify=On was requested.
        Assert.Empty(observer.Diagnostics);
    }

    // -----------------------------------------------------------------------
    //  9.6 — assert never appends to EventLog under any outcome
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.6  assert succeeds and fails: neither outcome produces a log entry.
    /// </summary>
    [Fact]
    public async Task Assert_NeverAppendsToEventLog()
    {
        var observer = new RecordingObserver();
        var executor = new BlockExecutor();

        // --- success path ---
        var (ctxOk, logOk, _) = BuildCtx(observer);
        var blockOk = AssertBlock(condition: true, onFail: OnFail.Continue, notify: NotifyFlag.On);
        await executor.ExecuteBlock(blockOk, ctxOk);

        // --- failure path (continue so no exception) ---
        var (ctxFail, logFail, _) = BuildCtx(observer);
        var blockFail = AssertBlock(condition: false, onFail: OnFail.Continue, notify: NotifyFlag.On);
        await executor.ExecuteBlock(blockFail, ctxFail);

        // Neither log should contain an "assert" event.
        Assert.DoesNotContain(logOk.ThisGame,   e => e.KeywordName == "assert");
        Assert.DoesNotContain(logFail.ThisGame, e => e.KeywordName == "assert");
    }

    // -----------------------------------------------------------------------
    //  Bonus: assert(true) never calls OnDiagnostic
    // -----------------------------------------------------------------------

    /// <summary>
    /// assert(true, …): condition passes — OnDiagnostic is never called.
    /// </summary>
    [Fact]
    public async Task Assert_WhenConditionTrue_NeverCallsOnDiagnostic()
    {
        var observer = new RecordingObserver();
        var (ctx, _, _) = BuildCtx(observer);
        var executor = new BlockExecutor();

        // All flag combinations with a passing condition.
        foreach (var onFail in Enum.GetValues<OnFail>())
        foreach (var notify in Enum.GetValues<NotifyFlag>())
        {
            var block = AssertBlock(condition: true, onFail: onFail, notify: notify);
            await executor.ExecuteBlock(block, ctx); // must not throw
        }

        // No diagnostics for any passing assert.
        Assert.Empty(observer.Diagnostics);
    }
}
