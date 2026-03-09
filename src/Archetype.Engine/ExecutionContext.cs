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

    /// <summary>
    /// Optional engine observer, used by the <c>assert</c> built-in to deliver
    /// <see cref="DiagnosticEvent"/> notifications when <c>notify: on</c>.
    /// May be <c>null</c> — all observer call sites must null-check.
    /// </summary>
    public IEngineObserver? Observer { get; }

    /// <summary>The complete game definition (immutable reference data).</summary>
    public GameDefinition Definition { get; }

    /// <summary>
    /// Name of the player whose turn it is, or whose action is being resolved.
    /// Used to dispatch prompts to the correct strategy.
    /// </summary>
    public string ActivePlayerName { get; }

    /// <summary>
    /// When <c>true</c>, the current block is a <see cref="CostDef.Body"/> execution.
    /// Under this flag, every <c>assert</c> call ignores its <c>on_fail</c> and
    /// <c>notify</c> arguments and behaves as <c>panic / off</c>: raises
    /// <see cref="EngineException"/> silently without calling
    /// <see cref="IEngineObserver.OnDiagnostic"/>.
    /// This ensures cost bodies always fail hard and silently on unaffordable state.
    /// </summary>
    public bool IsCostBody { get; set; }

    // -----------------------------------------------------------------------

    public ExecutionContext(
        GameState gameState,
        EventLog eventLog,
        Dictionary<string, object> bindings,
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        GameDefinition definition,
        string activePlayerName,
        IEngineObserver? observer = null)
    {
        GameState        = gameState;
        EventLog         = eventLog;
        Bindings         = bindings;
        Strategies       = strategies;
        RandomSource     = randomSource;
        Definition       = definition;
        ActivePlayerName = activePlayerName;
        Observer         = observer;
    }

    /// <summary>
    /// Creates a child context for a new action scope (used when firing a
    /// trigger).  The new context inherits GameState, EventLog, Strategies,
    /// RandomSource, Definition, Observer, and ActivePlayerName but has a fresh
    /// Bindings dictionary pre-populated with <paramref name="prePopulated"/>.
    /// <c>IsCostBody</c> is always <c>false</c> on the child — cost-body mode
    /// does not propagate across action scopes.
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
            ActivePlayerName,
            Observer);
        // IsCostBody defaults to false — intentionally not propagated.
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
