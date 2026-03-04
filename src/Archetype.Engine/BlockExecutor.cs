using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Executes <see cref="EffectBlockDef"/> values against an
/// <see cref="ExecutionContext"/>.
/// <para>
/// A block executes atomically: triggers and state-based effects do not
/// resolve mid-block.  The <see cref="BlockExecutor"/> does not call
/// <c>CheckLifetimes</c> or the trigger resolver directly — that is the
/// responsibility of <c>ActionResolver</c>, which calls those after each
/// block returns.
/// </para>
/// </summary>
internal sealed class BlockExecutor
{
    private readonly KeywordEvaluator _evaluator;
    private readonly MutationDispatch _mutations;
    private readonly PropertyDispatch _properties;

    public BlockExecutor()
    {
        _mutations  = new MutationDispatch();
        _properties = new PropertyDispatch();

        // Register all built-in handlers.
        BuiltInHandlers.Register(_mutations, _properties);

        // Task 5.4 — startup assertion: verify every BuiltInKeywords entry
        // has a registered implementation and no extra entries exist.
        BuiltInHandlers.AssertSync(_mutations, _properties);

        _evaluator = new KeywordEvaluator(_mutations, _properties);
    }

    // -----------------------------------------------------------------------
    //  Block execution (D3)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Executes all steps of <paramref name="block"/> against
    /// <paramref name="ctx"/>.  Manages the block's event log scope (open
    /// before first step, close after last step).
    /// <para>
    /// Returns <c>Task.CompletedTask</c> — the <c>async</c> signature is
    /// present for compatibility with prompt suspension; most blocks execute
    /// synchronously.
    /// </para>
    /// </summary>
    public async Task ExecuteBlock(EffectBlockDef block, ExecutionContext ctx)
    {
        ctx.EventLog.OpenBlock();
        try
        {
            foreach (var step in block.Steps)
                await ExecuteStep(step, ctx);
        }
        finally
        {
            ctx.EventLog.CloseBlock();
        }
    }

    // -----------------------------------------------------------------------
    //  Step execution
    // -----------------------------------------------------------------------

    private async Task ExecuteStep(EffectBlockStep step, ExecutionContext ctx)
    {
        // Resolve argument nodes.
        var args = step.ArgNodes
            .Select(n => _evaluator.EvaluateNode(n, ctx))
            .ToArray();

        object? result;

        if (_mutations.Has(step.KeywordName))
        {
            // Mutation dispatch goes through the evaluator (which applies
            // parameter modifications) — route through EvaluateNode on a
            // pre-constructed Invocation node.
            result = _evaluator.EvaluateNode(
                new Invocation(step.KeywordName, step.ArgNodes), ctx);
        }
        else if (_properties.Has(step.KeywordName))
        {
            // Property keywords at block-step level are unusual (most appear
            // as sub-expressions) but valid.
            result = _properties.Dispatch(step.KeywordName, args!, ctx);
        }
        else
        {
            // Composite keyword or unknown.
            result = _evaluator.EvaluateNode(
                new Invocation(step.KeywordName, step.ArgNodes), ctx);
        }

        // Bind the return value if the step declares a BindTo target.
        if (step.BindTo is not null && result is not null)
            ctx.Bindings[step.BindTo] = result;

        // Prompt suspension: if the result is a Task (async property keyword),
        // await it here.  In practice, only IPlayerStrategy calls suspend.
        if (result is Task t) await t;
    }

    // -----------------------------------------------------------------------
    //  Property evaluation in read-only mode (for conditions, while-checks)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Evaluates a boolean property expression against the given game state
    /// without a full <see cref="ExecutionContext"/> (used for while-condition
    /// checks and trigger condition evaluation).
    /// </summary>
    public bool EvaluateCondition(
        KeywordNode expression,
        GameState state,
        IReadOnlyDictionary<string, object>? bindings = null)
    {
        var b = bindings ?? new Dictionary<string, object>();
        var result = _evaluator.EvaluatePropertyNode(expression, b, state);
        return result is bool boolResult && boolResult;
    }
}
