using Archetype.Core;

namespace Archetype.Engine;

// ---------------------------------------------------------------------------
//  ActionResolver — post-action sequence orchestrator (D7)
// ---------------------------------------------------------------------------

/// <summary>
/// Orchestrates the full post-action sequence defined in D7:
/// <list type="number">
///   <item>Execute the primary effect block.</item>
///   <item>Call <c>CheckLifetimes</c> and <c>RunStateBasedRules</c> (fixpoint).</item>
///   <item>Cascade loop: collect satisfied triggers, fire each, run SBRs after
///   each fired block, repeat until the loop quiesces or the observer halts it.</item>
/// </list>
/// <para>
/// Create instances via <see cref="Create"/> (convenience factory that wires
/// a shared <see cref="BlockExecutor"/> between <c>ActionResolver</c> and
/// <see cref="TriggerResolver"/>), or via the constructor directly if you need
/// finer control over the injected components (e.g., testing sub-components
/// in isolation).
/// </para>
/// </summary>
internal sealed class ActionResolver
{
    private readonly BlockExecutor _executor;
    private readonly LifetimeChecker _lifetimes;
    private readonly TriggerResolver _triggers;
    private readonly GameDefinition _definition;
    private readonly IReadOnlyDictionary<string, IPlayerStrategy> _strategies;
    private readonly IRandomSource _randomSource;
    private readonly IEngineObserver? _observer;

    /// <summary>
    /// Constructs an <see cref="ActionResolver"/> with an externally-created
    /// <see cref="TriggerResolver"/> (and therefore an externally-created
    /// <see cref="BlockExecutor"/>).  <c>ActionResolver</c> will also create its
    /// own <see cref="BlockExecutor"/> for the primary block and SBR blocks; the
    /// two instances are functionally identical because <see cref="BlockExecutor"/>
    /// is stateless beyond its registered built-in handlers.
    /// </summary>
    public ActionResolver(
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        IEngineObserver? observer,
        TriggerResolver triggerResolver,
        GameDefinition definition)
    {
        _strategies   = strategies;
        _randomSource = randomSource;
        _observer     = observer;
        _triggers     = triggerResolver;
        _definition   = definition;
        // Independent executor/lifetimes for primary and SBR blocks; shares no
        // mutable state with TriggerResolver's executor (both are stateless).
        _executor  = new BlockExecutor();
        _lifetimes = new LifetimeChecker(_executor);
    }

    // -----------------------------------------------------------------------
    //  Convenience factory (D9 canonical constructor pattern)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Evaluates a pure <see cref="KeywordNode"/> boolean expression against
    /// the given state and optional bindings.  Delegates to the internal
    /// <see cref="BlockExecutor"/> so callers (e.g. <c>GameSession</c>) can
    /// evaluate activation conditions without access to the private executor.
    /// </summary>
    public bool EvaluateCondition(
        KeywordNode expression,
        GameState state,
        IReadOnlyDictionary<string, object>? bindings = null) =>
        _executor.EvaluateCondition(expression, state, bindings);

    /// <summary>
    /// Convenience factory: constructs a <see cref="BlockExecutor"/> shared with
    /// <see cref="TriggerResolver"/>, then returns a fully wired
    /// <see cref="ActionResolver"/> (which creates its own second executor for
    /// primary and SBR blocks).
    /// <para>
    /// Use this in tests and <c>GameSession</c> bootstrap.
    /// </para>
    /// <para>
    /// <b>Note on <c>AssertSync</c>:</b> Each <see cref="BlockExecutor"/> runs
    /// <c>AssertSync</c> at construction time.  <c>Create</c> produces two
    /// instances — one passed to <see cref="TriggerResolver"/> and one created
    /// by the <see cref="ActionResolver"/> constructor — so <c>AssertSync</c>
    /// effectively runs twice per <c>Create</c> call.  This is intentional: both
    /// executors are stateless beyond handler registration, and the double-check
    /// is harmless.
    /// </para>
    /// </summary>
    public static ActionResolver Create(
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        GameDefinition definition,
        IEngineObserver? observer = null)
    {
        var executor  = new BlockExecutor();
        var lifetimes = new LifetimeChecker(executor);
        var triggers  = new TriggerResolver(executor, lifetimes);
        return new ActionResolver(strategies, randomSource, observer, triggers, definition);
    }

    // -----------------------------------------------------------------------
    //  ResolveAction — full D7 post-action sequence
    // -----------------------------------------------------------------------

    /// <summary>
    /// Runs the full post-action sequence for a single player action.
    /// </summary>
    /// <param name="primaryBlock">
    /// The effect block to execute as the primary action.  Pass <c>null</c> for
    /// pass-actions where no block executes but triggers and SBRs still resolve.
    /// </param>
    /// <param name="state">The mutable game state.</param>
    /// <param name="eventLog">The session event log.</param>
    /// <param name="activePlayerName">
    /// Name of the player whose action this is; used to route prompts via
    /// <see cref="IPlayerStrategy"/>.
    /// </param>
    /// <param name="currentTurn">
    /// The current turn number; forwarded to <c>CheckLifetimes</c> for
    /// <see cref="TurnTimer"/> evaluation.
    /// </param>
    /// <param name="costBlocks">
    /// Optional cost bodies to execute before <paramref name="primaryBlock"/>
    /// within the same action scope (D23).  Each cost body runs with
    /// <c>IsCostBody = true</c>.
    /// </param>
    public async Task ResolveAction(
        EffectBlockDef? primaryBlock,
        GameState state,
        EventLog eventLog,
        string activePlayerName,
        int currentTurn,
        IReadOnlyList<EffectBlockDef>? costBlocks = null)
    {
        var ctx = new ExecutionContext(
            state, eventLog,
            new Dictionary<string, object>(),
            _strategies, _randomSource, _definition,
            activePlayerName,
            observer: _observer);

        // ── Step 1: cost bodies then primary block ───────────────────────────
        // All run inside the same action scope so cost events and primary events
        // share events.this_action (D23).
        eventLog.OpenAction();
        try
        {
            // Execute cost bodies first, with IsCostBody = true (D20).
            if (costBlocks is { Count: > 0 })
            {
                ctx.IsCostBody = true;
                foreach (var costBlock in costBlocks)
                    await _executor.ExecuteBlock(costBlock, ctx);
                ctx.IsCostBody = false;
            }

            if (primaryBlock is not null)
                await _executor.ExecuteBlock(primaryBlock, ctx);
        }
        finally
        {
            // Always reset IsCostBody and close the action scope.
            ctx.IsCostBody = false;
            eventLog.CloseAction();
        }

        _lifetimes.CheckLifetimes(state, currentTurn);

        // ── Step 2: state-based rules before cascade (fixpoint) ──────────────
        await RunStateBasedRules(ctx, currentTurn);

        // ── Steps 3–9: cascade loop ──────────────────────────────────────────
        // triggerBatchCount is local to this action; 1-based when passed to
        // the observer so the host sees "first batch" as 1, not 0 (D7 §observer).
        int triggerBatchCount = 0;

        while (true)
        {
            // Exit the cascade if the game ended (e.g. declare-winner in a trigger
            // body), so we don't collect further triggers or call the observer
            // after the game is already decided.
            if (state.GameIsOver) break;

            triggerBatchCount++;

            // Step 4: ask the observer whether to continue.  null → always Continue.
            var directive = _observer is null
                ? CascadeDirective.Continue
                : await _observer.OnTriggerCascadeAsync(triggerBatchCount);

            if (directive == CascadeDirective.Halt) break;

            // Step 6: collect all triggers satisfied by events produced since
            // each effect's high-water mark; sort by the game's resolution order.
            var firings = _triggers.CollectSatisfiedTriggers(state, eventLog);
            var ordered = TriggerResolver.OrderTriggerFirings(
                firings, _definition.TriggerResolutionOrder);

            // Step 7: no new triggers → cascade has quiesced.
            if (ordered.Count == 0) break;

            // Step 8: fire each trigger in order.
            // FireTrigger increments TriggerFireCount BEFORE executing the block
            // (D8), manages its own action scope, and calls CheckLifetimes.
            // We call RunStateBasedRules after every fired block (D7 step 8).
            foreach (var firing in ordered)
            {
                await _triggers.FireTrigger(firing, ctx, currentTurn);
                await RunStateBasedRules(ctx, currentTurn);
                // If a trigger or its resulting SBRs ended the game, stop firing
                // remaining triggers in this batch — they belong to a game that's over.
                if (state.GameIsOver) break;
            }
            // Loop back to collect triggers produced by the blocks just fired.
        }
    }

    // -----------------------------------------------------------------------
    //  RunStateBasedRules — fixpoint loop (D7)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Evaluates all <see cref="StateBasedRule"/> conditions in a fixpoint loop:
    /// collect every currently satisfied rule, fire their blocks (with a
    /// <c>CheckLifetimes</c> call after each), then re-evaluate until a pass
    /// produces no satisfied rules.
    /// <para>
    /// All conditions for a pass are evaluated before any blocks fire, ensuring
    /// that rules which "undo" each other's preconditions don't oscillate within
    /// a single pass.
    /// </para>
    /// </summary>
    private async Task RunStateBasedRules(ExecutionContext ctx, int currentTurn)
    {
        while (true)
        {
            // If the game ended mid-pass (e.g. declare-winner/declare-draw was
            // called by a rule body), stop immediately.  The condition that fired
            // the terminal rule may still evaluate true, which would otherwise
            // produce an infinite loop.
            if (ctx.GameState.GameIsOver) return;

            // Snapshot satisfied rules BEFORE firing to avoid mid-pass re-evaluation.
            var triggered = ctx.Definition.StateBasedRules
                .Where(rule => _executor.EvaluateCondition(rule.Condition, ctx.GameState))
                .ToList();

            if (triggered.Count == 0) return;

            foreach (var rule in triggered)
            {
                // Each SBR block runs in its own action scope so its events are
                // committed to _turnAccum before the next block runs.  This
                // prevents OpenAction() in the cascade from silently discarding
                // uncommitted SBR events.
                ctx.EventLog.OpenAction();
                try
                {
                    await _executor.ExecuteBlock(rule.Body, ctx);
                }
                finally
                {
                    ctx.EventLog.CloseAction();
                }
                _lifetimes.CheckLifetimes(ctx.GameState, currentTurn);
            }
            // Re-evaluate: blocks above may have satisfied or resolved other rules.
        }
    }
}
