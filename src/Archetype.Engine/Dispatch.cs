using Archetype.Core;

namespace Archetype.Engine;

// ---------------------------------------------------------------------------
//  Dispatch tables for mutation and property keywords
// ---------------------------------------------------------------------------

/// <summary>
/// Handler signature for mutation keywords.  A mutation keyword has side
/// effects on <see cref="GameState"/> and may append events to the
/// <see cref="EventLog"/>.  Returns a value for keywords like
/// <c>apply-modifier</c> (returns a <see cref="ContributionId"/>); void-like
/// keywords return <c>null</c>.
/// </summary>
internal delegate object? MutationHandler(object[] args, ExecutionContext ctx);

/// <summary>
/// Handler signature for property keywords that need full execution context
/// (e.g. for <c>IRandomSource</c> access).
/// </summary>
internal delegate object? PropertyHandler(object[] args, ExecutionContext ctx);

/// <summary>
/// Handler signature for property keywords evaluated in read-only mode
/// (trigger conditions, while-condition checks — no <see cref="ExecutionContext"/>).
/// </summary>
internal delegate object? PurePropertyHandler(
    object[] args, GameState state, IReadOnlyDictionary<string, object> bindings);

/// <summary>
/// Registry and dispatcher for mutation (side-effecting) keywords.
/// </summary>
internal sealed class MutationDispatch
{
    private readonly Dictionary<string, MutationHandler> _handlers = new(StringComparer.Ordinal);

    /// <summary>Registers a mutation handler under the given keyword name.</summary>
    public void Register(string name, MutationHandler handler)
    {
        if (!_handlers.TryAdd(name, handler))
            throw new InvalidOperationException($"Mutation handler for '{name}' is already registered.");
    }

    /// <summary>Returns <c>true</c> if the keyword has a registered mutation handler.</summary>
    public bool Has(string name) => _handlers.ContainsKey(name);

    /// <summary>Dispatches to the registered mutation handler.</summary>
    public object? Dispatch(string name, object[] args, ExecutionContext ctx)
    {
        if (_handlers.TryGetValue(name, out var h)) return h(args, ctx);
        throw new EngineException($"No mutation handler registered for keyword '{name}'.");
    }

    /// <summary>Returns the names of all registered mutation keywords.</summary>
    public IEnumerable<string> RegisteredNames => _handlers.Keys;
}

/// <summary>
/// Registry and dispatcher for property (read-only) keywords.  A property
/// keyword returns a value and has no side effects on game state.
/// </summary>
internal sealed class PropertyDispatch
{
    private readonly Dictionary<string, PropertyHandler>     _full  = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PurePropertyHandler> _pure  = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a property handler.  Optionally provides a pure-mode variant
    /// for use in trigger conditions and while-condition checks.
    /// </summary>
    public void Register(string name, PropertyHandler handler, PurePropertyHandler? pureHandler = null)
    {
        if (!_full.TryAdd(name, handler))
            throw new InvalidOperationException($"Property handler for '{name}' is already registered.");
        if (pureHandler is not null) _pure[name] = pureHandler;
    }

    /// <summary>Returns <c>true</c> if the keyword has a registered property handler.</summary>
    public bool Has(string name) => _full.ContainsKey(name);

    /// <summary>Dispatches using full execution context.</summary>
    public object? Dispatch(string name, object[] args, ExecutionContext ctx)
    {
        if (_full.TryGetValue(name, out var h)) return h(args, ctx);
        throw new EngineException($"No property handler registered for keyword '{name}'.");
    }

    /// <summary>
    /// Dispatches in read-only (pure) mode.
    /// Throws if no pure handler is registered for the given keyword.
    /// </summary>
    public object? DispatchPure(string name, object[] args, GameState state, IReadOnlyDictionary<string, object> bindings)
    {
        if (_pure.TryGetValue(name, out var pure)) return pure(args, state, bindings);
        if (_full.TryGetValue(name, out _))
            throw new EngineException($"Property keyword '{name}' is not available in pure (read-only) evaluation mode.");
        throw new EngineException($"No property handler registered for keyword '{name}'.");
    }

    /// <summary>Returns the names of all registered property keywords.</summary>
    public IEnumerable<string> RegisteredNames => _full.Keys;
}
