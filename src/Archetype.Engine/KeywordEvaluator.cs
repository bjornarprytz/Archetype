using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Resolves <see cref="KeywordNode"/> trees to runtime values and dispatches
/// mutation / property keywords.
/// <para>
/// Property keywords return values; mutation keywords mutate <see cref="GameState"/>
/// and append events to the <see cref="EventLog"/>.  The distinction is
/// enforced at authoring time by the type system; at runtime the evaluator
/// dispatches based on whether a registered handler is a mutation or property handler.
/// </para>
/// </summary>
internal sealed class KeywordEvaluator
{
    private readonly MutationDispatch   _mutations;
    private readonly PropertyDispatch   _properties;

    public KeywordEvaluator(MutationDispatch mutations, PropertyDispatch properties)
    {
        _mutations  = mutations;
        _properties = properties;
    }

    // -----------------------------------------------------------------------
    //  Node evaluation (D2, D3)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Evaluates a <see cref="KeywordNode"/> to a runtime value.
    /// Mutation invocations have side effects; property invocations are pure.
    /// </summary>
    public object? EvaluateNode(KeywordNode node, ExecutionContext ctx)
    {
        return node switch
        {
            Literal lit   => lit.Value,
            ParameterRef p => ResolveParam(p.Name, ctx),
            Invocation inv => EvaluateInvocation(inv, ctx),
            _ => throw new EngineException($"Unknown KeywordNode type: {node.GetType().Name}"),
        };
    }

    /// <summary>
    /// Evaluates a property-only node (no side effects permitted).
    /// Used for condition expressions, while-condition checks, etc.
    /// </summary>
    public object? EvaluatePropertyNode(KeywordNode node, IReadOnlyDictionary<string, object> bindings, GameState state)
    {
        return node switch
        {
            Literal lit   => lit.Value,
            ParameterRef p => bindings.TryGetValue(p.Name, out var v)
                                  ? v
                                  : throw new EngineException($"Unbound parameter '{p.Name}' in property expression."),
            Invocation inv => EvaluatePropertyInvocation(inv, bindings, state),
            _ => throw new EngineException($"Unknown KeywordNode type in property expression: {node.GetType().Name}"),
        };
    }

    // -----------------------------------------------------------------------
    //  Private helpers
    // -----------------------------------------------------------------------

    private object? ResolveParam(string name, ExecutionContext ctx)
    {
        if (ctx.Bindings.TryGetValue(name, out var v)) return v;
        throw new EngineException($"Unbound parameter '{name}' in execution context.");
    }

    private object? EvaluateInvocation(Invocation inv, ExecutionContext ctx)
    {
        var args = inv.Args.Select(a => EvaluateNode(a, ctx)).ToArray();

        // Check if this is a mutation keyword — apply parameter modifications first (D13).
        if (_mutations.Has(inv.KeywordName))
        {
            var modifiedArgs = ApplyParameterModifications(inv.KeywordName, args, ctx);
            if (modifiedArgs is null)
            {
                // A Disable fired — log keyword-disabled event.
                var disabledArgs = BuildBoundArgs(inv.KeywordName, args, ctx.Definition);
                disabledArgs["keyword"] = inv.KeywordName;
                var disabledEvent = new GameEvent
                {
                    KeywordName = "keyword-disabled",
                    BoundArgs   = disabledArgs,
                };
                ctx.EventLog.Append(disabledEvent);
                return null;
            }

            // Wrap single object back to array if needed.
            var finalArgs = modifiedArgs as object?[] ?? args;
            return _mutations.Dispatch(inv.KeywordName, finalArgs!, ctx);
        }

        if (_properties.Has(inv.KeywordName))
            return _properties.Dispatch(inv.KeywordName, args!, ctx);

        // Could be a game-creator composite keyword — look it up in the definition.
        if (ctx.Definition.Keywords.TryGetValue(inv.KeywordName, out var kwDef) && !kwDef.IsPrimitive)
            return EvaluateComposite(kwDef, args!, ctx);

        throw new EngineException($"Unknown keyword '{inv.KeywordName}'.");
    }

    private object? EvaluatePropertyInvocation(Invocation inv, IReadOnlyDictionary<string, object> bindings, GameState state)
    {
        var args = inv.Args.Select(a => EvaluatePropertyNode(a, bindings, state)).ToArray();

        if (_properties.Has(inv.KeywordName))
            return _properties.DispatchPure(inv.KeywordName, args!, state, bindings);

        throw new EngineException($"Unknown property keyword '{inv.KeywordName}' in property expression.");
    }

    /// <summary>
    /// Evaluates a composite keyword by substituting parameter bindings and
    /// recursively evaluating its body.
    /// <para>
    /// Uses <see cref="EventLog.PushCompositeParent"/> so that all events
    /// produced during body execution are appended as children of the
    /// composite wrapper event rather than directly into the flat block
    /// accumulator.  This prevents duplicates in scope queries (D4).
    /// </para>
    /// </summary>
    private object? EvaluateComposite(KeywordDefinition def, object?[] args, ExecutionContext ctx)
    {
        // Bind call arguments to the keyword's declared parameter names.
        var innerBindings = new Dictionary<string, object>(ctx.Bindings);
        for (int i = 0; i < def.Parameters.Length && i < args.Length; i++)
            if (args[i] is not null)
                innerBindings[def.Parameters[i].Name] = args[i]!;

        var compositeEvent = new GameEvent
        {
            KeywordName = def.Name,
            BoundArgs   = BuildBoundArgsFromArgs(def, args!),
        };

        // Push the composite onto the EventLog parent stack.  From this point,
        // any Append call will nest the event under compositeEvent rather than
        // adding it to the flat block accumulator.
        ctx.EventLog.PushCompositeParent(compositeEvent);

        var innerCtx = new ExecutionContext(
            ctx.GameState, ctx.EventLog, innerBindings,
            ctx.Strategies, ctx.RandomSource, ctx.Definition, ctx.ActivePlayerName);

        object? result = null;
        try
        {
            if (def.Body is not null)
                result = EvaluateNode(def.Body, innerCtx);
        }
        finally
        {
            // Always pop — even if the body throws — to keep the stack consistent.
            ctx.EventLog.PopCompositeParent();
        }

        // Composite event now owns all children; append it flat to the block.
        ctx.EventLog.Append(compositeEvent);

        return result;
    }

    // -----------------------------------------------------------------------
    //  Parameter modification application (D13) — stub: always returns args
    // -----------------------------------------------------------------------

    /// <summary>
    /// Applies active <see cref="ParameterModification"/> static effects to the
    /// given mutation keyword's arguments.  Returns <c>null</c> if a
    /// <see cref="Disable"/> fires; otherwise returns the (possibly modified) args.
    /// </summary>
    private object?[]? ApplyParameterModifications(string keyword, object?[] args, ExecutionContext ctx)
    {
        // Collect matching static effects that target this keyword.
        var matching = ctx.GameState.ActiveStaticEffects
            .Where(se => se.ParameterModification?.TargetKeyword == keyword)
            .OrderBy(se => se.Id.Value)
            .ToList();

        if (matching.Count == 0) return args; // Fast path — no modifications.

        // Evaluate filter conditions and collect applicable modifications.
        var applicable = new List<(StaticEffect se, ParameterModification pm)>();
        foreach (var se in matching)
        {
            var pm = se.ParameterModification!;
            var evalBindings = new Dictionary<string, object> { ["source"] = se.OwnerAtom };
            // Bind arg-filter-declared names from the invocation's args.
            BindArgFilter(pm.ArgFilter, args, keyword, evalBindings, ctx.Definition);
            bool passes = pm.FilterCondition is null ||
                          EvaluatePropertyNode(pm.FilterCondition, evalBindings, ctx.GameState) is true;
            if (passes) applicable.Add((se, pm));
        }

        // Step 1: Check for any Disable.
        if (applicable.Any(x => x.pm is Disable)) return null;

        // Step 2: Apply ParameterAdjustments (Additive → Multiplicative → Replace).
        var resultArgs = (object?[])args.Clone();
        var kwDef = ctx.Definition.Keywords.TryGetValue(keyword, out var def) ? def : null;
        int paramCount = kwDef?.Parameters.Length ?? resultArgs.Length;

        for (int i = 0; i < paramCount && i < resultArgs.Length; i++)
        {
            string paramName = kwDef?.Parameters[i].Name ?? i.ToString();
            var raw = resultArgs[i];

            var paramMods = applicable
                .Where(x => x.pm is ParameterAdjustment)
                .SelectMany(x => ((ParameterAdjustment)x.pm).ParamMods)
                .Where(m => m.ParamName == paramName)
                .ToList();

            if (paramMods.Count == 0) continue;

            // Only apply to numeric values for Additive/Multiplicative.
            if (raw is not double rawNum)
            {
                // Replace still works on non-numeric values.
                foreach (var mod in paramMods.Where(m => m.Kind == ParamModKind.Replace))
                {
                    var evalBindings = new Dictionary<string, object> { ["original"] = raw! };
                    resultArgs[i] = EvaluatePropertyNode(mod.Expression, evalBindings, ctx.GameState);
                }
                continue;
            }

            double additive = paramMods.Where(m => m.Kind == ParamModKind.Additive)
                .Sum(m => AsDouble(EvaluatePropertyNode(m.Expression,
                    new Dictionary<string, object> { ["original"] = rawNum }, ctx.GameState)));

            double multiplicative = paramMods.Where(m => m.Kind == ParamModKind.Multiplicative)
                .Aggregate(1.0, (p, m) => p * AsDouble(EvaluatePropertyNode(m.Expression,
                    new Dictionary<string, object> { ["original"] = rawNum }, ctx.GameState)));

            double running = (rawNum + additive) * multiplicative;

            foreach (var mod in paramMods.Where(m => m.Kind == ParamModKind.Replace))
                running = AsDouble(EvaluatePropertyNode(mod.Expression,
                    new Dictionary<string, object> { ["original"] = running }, ctx.GameState));

            resultArgs[i] = running;
        }

        return resultArgs;
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static void BindArgFilter(
        IReadOnlyList<EventParamDecl>? argFilter,
        object?[] args, string keyword,
        Dictionary<string, object> bindings,
        GameDefinition def)
    {
        if (argFilter is null) return;
        if (!def.Keywords.TryGetValue(keyword, out var kwDef)) return;

        foreach (var decl in argFilter)
        {
            int idx = Array.FindIndex(kwDef.Parameters, p => p.Name == decl.ArgName);
            if (idx >= 0 && idx < args.Length && args[idx] is not null)
                bindings[decl.ParamName] = args[idx]!;
        }
    }

    private static double AsDouble(object? v) => v switch
    {
        double d   => d,
        int i      => i,
        long l     => l,
        float f    => f,
        bool b     => b ? 1.0 : 0.0,
        null       => 0.0,
        _          => throw new EngineException($"Expected a number but got {v.GetType().Name}."),
    };

    private static Dictionary<string, object> BuildBoundArgs(
        string keyword, object?[] args, GameDefinition def)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (!def.Keywords.TryGetValue(keyword, out var kwDef)) return result;
        for (int i = 0; i < kwDef.Parameters.Length && i < args.Length; i++)
            if (args[i] is not null) result[kwDef.Parameters[i].Name] = args[i]!;
        return result;
    }

    private static IReadOnlyDictionary<string, object> BuildBoundArgsFromArgs(
        KeywordDefinition def, object?[] args)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        for (int i = 0; i < def.Parameters.Length && i < args.Length; i++)
            if (args[i] is not null) result[def.Parameters[i].Name] = args[i]!;
        return result;
    }
}
