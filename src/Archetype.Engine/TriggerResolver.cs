using Archetype.Core;

namespace Archetype.Engine;

// ---------------------------------------------------------------------------
//  TriggerFiring — a single pending trigger execution
// ---------------------------------------------------------------------------

/// <summary>
/// Pairs a static effect whose trigger condition was satisfied with the
/// specific candidate <see cref="GameEvent"/> that satisfied it.
/// <para>
/// Produced by <see cref="TriggerResolver.CollectSatisfiedTriggers"/> and
/// consumed by <see cref="TriggerResolver.FireTrigger"/>.
/// </para>
/// </summary>
internal sealed record TriggerFiring(StaticEffect Effect, GameEvent Event);

// ---------------------------------------------------------------------------
//  TriggerResolver
// ---------------------------------------------------------------------------

/// <summary>
/// Collects, orders, and fires trigger effects after each action in the
/// post-action cascade loop (D7, D8).
/// <para>
/// Responsibilities:
/// <list type="number">
///   <item>Scan active static effects for satisfied trigger conditions.</item>
///   <item>Advance each effect's <c>TriggerHighWaterMark</c> past all seen
///   candidate events (matched or not) to prevent re-firing.</item>
///   <item>Sort the collected firings according to
///   <see cref="GameDefinition.TriggerResolutionOrder"/>.</item>
///   <item>Execute each firing's <c>FiredBlock</c> in a fresh child action
///   scope, incrementing <c>TriggerFireCount</c> before execution so
///   <c>CheckLifetimes</c> can evaluate <c>TriggerCount</c> correctly.</item>
/// </list>
/// </para>
/// </summary>
internal sealed class TriggerResolver
{
    private readonly BlockExecutor _executor;
    private readonly LifetimeChecker _lifetimes;

    /// <summary>
    /// Constructs a <see cref="TriggerResolver"/> sharing the given
    /// <paramref name="executor"/> and <paramref name="lifetimes"/> with the
    /// parent <see cref="ActionResolver"/>.
    /// </summary>
    public TriggerResolver(BlockExecutor executor, LifetimeChecker lifetimes)
    {
        _executor = executor;
        _lifetimes = lifetimes;
    }

    // -----------------------------------------------------------------------
    //  Task 1.2 — CollectSatisfiedTriggers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Scans all active static effects with triggers and returns the list of
    /// firings whose conditions are satisfied by events not yet seen by the
    /// effect's high-water mark.
    /// <para>
    /// As a side effect, advances each evaluated effect's
    /// <see cref="StaticEffect.TriggerHighWaterMark"/> past every candidate
    /// event it inspected, whether matched or not.  This guarantees "at most
    /// once per event" firing (D8 §high-water mark).
    /// </para>
    /// <para>
    /// Results are returned unordered; call
    /// <see cref="OrderTriggerFirings"/> before firing.
    /// </para>
    /// </summary>
    public List<TriggerFiring> CollectSatisfiedTriggers(GameState state, EventLog eventLog)
    {
        var result = new List<TriggerFiring>();

        foreach (var se in state.ActiveStaticEffects)
        {
            var trigger = se.Trigger;
            if (trigger is null) continue;

            // Candidates: all events anywhere in the game (recursively, so
            // composite keyword children are visible) that match the trigger's
            // EventKeyword and haven't been seen by this effect before.
            // TriggerScope governs what the condition can query via log reads,
            // NOT which events are candidates — the high-water mark handles
            // that (D8: "does not restrict which events are candidates").
            var candidates = eventLog.ThisGame
                .Where(e => e.KeywordName == trigger.EventKeyword &&
                            e.SequenceNumber > se.TriggerHighWaterMark)
                .OrderBy(e => e.SequenceNumber)
                .ToList();

            long newHighWater = se.TriggerHighWaterMark;
            foreach (var ev in candidates)
            {
                // Advance past this candidate unconditionally — matched or not.
                newHighWater = ev.SequenceNumber;

                // Build condition bindings from EventParams mapping.
                var evalBindings = BuildEventParamBindings(trigger, ev);

                // null Condition = match all events of this keyword.
                bool matches = trigger.Condition is null ||
                               _executor.EvaluateCondition(trigger.Condition, state, evalBindings);

                if (matches)
                    result.Add(new TriggerFiring(se, ev));
            }

            // Advance high-water mark past ALL seen candidates, matched or not.
            // Events produced by THIS cascade batch's trigger blocks will have
            // higher sequence numbers and thus be visible in the NEXT pass.
            se.TriggerHighWaterMark = newHighWater;
        }

        return result;
    }

    // -----------------------------------------------------------------------
    //  Task 1.3 — OrderTriggerFirings
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sorts <paramref name="firings"/> according to the game's configured
    /// <see cref="TriggerResolutionOrder"/>.
    /// <para>
    /// <c>OldestFirst</c> (default): <c>(StaticEffectId ASC, SequenceNumber ASC)</c>.<br/>
    /// <c>OldestLast</c>: <c>(StaticEffectId DESC, SequenceNumber ASC)</c>.<br/>
    /// <c>PromptPlayer</c>: falls back to <c>OldestFirst</c> in this release
    /// (deferred per design Decision 5).
    /// </para>
    /// </summary>
    public static List<TriggerFiring> OrderTriggerFirings(
        List<TriggerFiring> firings,
        TriggerResolutionOrder order)
    {
        return order switch
        {
            TriggerResolutionOrder.OldestFirst or TriggerResolutionOrder.PromptPlayer =>
                // PromptPlayer deferred: fall back to OldestFirst.
                firings
                    .OrderBy(f => f.Effect.Id.Value)
                    .ThenBy(f => f.Event.SequenceNumber)
                    .ToList(),

            TriggerResolutionOrder.OldestLast =>
                firings
                    .OrderByDescending(f => f.Effect.Id.Value)
                    .ThenBy(f => f.Event.SequenceNumber)
                    .ToList(),

            _ => firings,
        };
    }

    // -----------------------------------------------------------------------
    //  Task 2.1 — FireTrigger
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires a single trigger: populates bindings, increments
    /// <see cref="StaticEffect.TriggerFireCount"/> (BEFORE execution so
    /// <c>CheckLifetimes</c> sees the updated count), executes the
    /// <c>FiredBlock</c> in a child action scope, then calls
    /// <c>CheckLifetimes</c>.
    /// </summary>
    public async Task FireTrigger(
        TriggerFiring firing,
        ExecutionContext parentCtx,
        int currentTurn)
    {
        var se      = firing.Effect;
        var ev      = firing.Event;
        var trigger = se.Trigger!;

        // Pre-populate the fired block's variable bindings.
        var bindings = new Dictionary<string, object>
        {
            // Reserved binding: the full triggering event is ALWAYS available (D8).
            ["trigger_event"] = new EventRef(ev),
        };

        // Apply any convenience EventBindings the game creator declared —
        // these let blocks reference specific args by friendly names.
        foreach (var b in trigger.EventBindings)
        {
            if (ev.BoundArgs.TryGetValue(b.EventArgName, out var val))
                bindings[b.BlockVarName] = val;
        }

        // Increment BEFORE ExecuteBlock: CheckLifetimes must see the updated
        // count so that TriggerCount(N) lifetime conditions expire correctly (D8).
        se.TriggerFireCount++;

        // Each trigger-fired block runs in a child action scope so its events
        // do not merge into the primary block's action scope (D7 step 8).
        var childCtx = parentCtx.CreateChildActionContext(bindings);
        parentCtx.EventLog.OpenAction();
        try
        {
            await _executor.ExecuteBlock(trigger.FiredBlock, childCtx);
        }
        finally
        {
            parentCtx.EventLog.CloseAction();
        }

        _lifetimes.CheckLifetimes(parentCtx.GameState, currentTurn);
    }

    // -----------------------------------------------------------------------
    //  Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps <see cref="TriggerDefinition.EventParams"/> onto the candidate
    /// event's <see cref="GameEvent.BoundArgs"/> to produce condition bindings.
    /// </summary>
    private static Dictionary<string, object> BuildEventParamBindings(
        TriggerDefinition trigger,
        GameEvent ev)
    {
        var bindings = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var decl in trigger.EventParams)
        {
            if (ev.BoundArgs.TryGetValue(decl.ArgName, out var val))
                bindings[decl.ParamName] = val;
        }
        return bindings;
    }
}
