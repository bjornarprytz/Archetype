using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Carries all mutable and injectable context for a single effect block
/// execution.  Passed through every interpreter call.
/// <para>
/// The scope IDs distinguish events from different blocks/actions/turns in
/// the event log.  Bindings accumulate as block steps execute; later steps
/// can reference earlier results via <see cref="ParameterRef"/> nodes.
/// </para>
/// </summary>
internal sealed class ExecutionContext
{
    /// <summary>The mutable game state being operated on.</summary>
    public GameState GameState { get; }

    /// <summary>The event log for this session.</summary>
    public EventLog EventLog { get; }

    /// <summary>
    /// Variable bindings for the current block scope.  Populated by prompt
    /// responses and by <c>BindTo</c> captures in <see cref="EffectBlockStep"/>.
    /// </summary>
    public Dictionary<string, object> Bindings { get; }

    /// <summary>
    /// Per-player strategies, keyed by player name.  Used by the
    /// <c>ActionResolver</c>; accessible here so keyword implementations
    /// can dispatch prompts.
    /// </summary>
    public IReadOnlyDictionary<string, IPlayerStrategy> Strategies { get; }

    /// <summary>The random source injected at session construction.</summary>
    public IRandomSource RandomSource { get; }

    /// <summary>The complete game definition (immutable reference data).</summary>
    public GameDefinition Definition { get; }

    /// <summary>
    /// Name of the player whose turn it is, or whose action is being resolved.
    /// Used to dispatch prompts to the correct strategy.
    /// </summary>
    public string ActivePlayerName { get; }

    // -----------------------------------------------------------------------

    public ExecutionContext(
        GameState gameState,
        EventLog eventLog,
        Dictionary<string, object> bindings,
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        GameDefinition definition,
        string activePlayerName)
    {
        GameState        = gameState;
        EventLog         = eventLog;
        Bindings         = bindings;
        Strategies       = strategies;
        RandomSource     = randomSource;
        Definition       = definition;
        ActivePlayerName = activePlayerName;
    }

    /// <summary>
    /// Creates a child context for a new action scope (used when firing a
    /// trigger).  The new context inherits GameState, EventLog, Strategies,
    /// RandomSource, and Definition but has a fresh Bindings dictionary
    /// pre-populated with <paramref name="prePopulated"/>.
    /// </summary>
    public ExecutionContext CreateChildActionContext(
        Dictionary<string, object>? prePopulated = null)
    {
        return new ExecutionContext(
            GameState,
            EventLog,
            prePopulated ?? new Dictionary<string, object>(),
            Strategies,
            RandomSource,
            Definition,
            ActivePlayerName);
    }

    /// <summary>
    /// Returns the strategy for the active player.  Throws
    /// <see cref="EngineException"/> if the player name is not registered.
    /// </summary>
    public IPlayerStrategy ActiveStrategy =>
        Strategies.TryGetValue(ActivePlayerName, out var s)
            ? s
            : throw new EngineException($"No strategy registered for player '{ActivePlayerName}'.");
}

// TriggerEvaluationContext was removed: trigger condition evaluation uses a plain
// Dictionary<string, object> populated by TriggerResolver.BuildEventParamBindings,
// which always includes "source" = ownerAtom plus any game-creator EventParams.
// BlockExecutor.EvaluateCondition already accepts IReadOnlyDictionary<string, object>,
// so no wrapper class is needed.
