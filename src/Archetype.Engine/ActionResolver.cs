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
    /// Convenience factory: constructs a single <see cref="BlockExecutor"/> and
    /// <see cref="LifetimeChecker"/>, creates a <see cref="TriggerResolver"/> from
    /// them, then returns a fully wired <see cref="ActionResolver"/>.
    /// <para>
    /// Use this in tests and <c>GameSession</c> bootstrap.  The shared executor
    /// ensures the built-in handler sync assertion runs exactly once.
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
    public async Task ResolveAction(
        EffectBlockDef? primaryBlock,
        GameState state,
        EventLog eventLog,
        string activePlayerName,
        int currentTurn)
    {
        var ctx = new ExecutionContext(
            state, eventLog,
            new Dictionary<string, object>(),
            _strategies, _randomSource, _definition,
            activePlayerName);

        // ── Step 1: primary block ────────────────────────────────────────────
        // Each primary block runs inside its own action scope so its events
        // are committed to the turn accumulator before trigger collection.
        eventLog.OpenAction();
        try
        {
            if (primaryBlock is not null)
                await _executor.ExecuteBlock(primaryBlock, ctx);
        }
        finally
        {
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
