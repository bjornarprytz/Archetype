using Archetype.Core;
using Archetype.Engine;

// Disambiguate from System.Threading.ExecutionContext.
using EngineContext = Archetype.Engine.ExecutionContext;
using EngineEventLog = Archetype.Engine.EventLog;

namespace Archetype.Tests.Helpers;

// ---------------------------------------------------------------------------
//  Test infrastructure helpers (D16)
// ---------------------------------------------------------------------------

/// <summary>
/// A scripted player strategy that drains pre-queued actions and responses.
/// Designed for Layer 2 and Layer 3 tests where player interactions are
/// deterministic.  Throws <see cref="InvalidOperationException"/> if a queue
/// is empty — that is a test-setup error, not an engine error.
/// </summary>
public sealed class ScriptedPlayerStrategy : IPlayerStrategy
{
    private readonly Queue<PlayerAction?> _actions   = new();
    private readonly Queue<PromptResponse> _responses = new();

    /// <summary>Enqueues an action for the next <c>SelectActionAsync</c> call.</summary>
    public ScriptedPlayerStrategy QueueAction(PlayerAction? action) { _actions.Enqueue(action); return this; }

    /// <summary>Enqueues a response for the next <c>RespondToPromptAsync</c> call.</summary>
    public ScriptedPlayerStrategy QueueResponse(PromptResponse response) { _responses.Enqueue(response); return this; }

    /// <inheritdoc/>
    public Task<PlayerAction?> SelectActionAsync(AvailableActions available, GameStateView state, CancellationToken ct = default) =>
        Task.FromResult(_actions.Count > 0
            ? _actions.Dequeue()
            : throw new InvalidOperationException("ScriptedPlayerStrategy: action queue is empty."));

    /// <inheritdoc/>
    public Task<PromptResponse> RespondToPromptAsync(PromptContext context, GameStateView state, CancellationToken ct = default) =>
        Task.FromResult(_responses.Count > 0
            ? _responses.Dequeue()
            : throw new InvalidOperationException("ScriptedPlayerStrategy: response queue is empty."));
}

/// <summary>
/// A mock random source that returns values from a pre-set queue.
/// Returns values regardless of min/max bounds — the test controls output.
/// </summary>
public sealed class MockRandomSource : IRandomSource
{
    private readonly Queue<int> _values = new();

    /// <summary>Enqueues values to be returned by subsequent <c>NextInt</c> calls.</summary>
    public MockRandomSource Enqueue(params int[] values)
    {
        foreach (var v in values) _values.Enqueue(v);
        return this;
    }

    /// <inheritdoc/>
    public int NextInt(int minInclusive, int maxInclusive) =>
        _values.Count > 0
            ? _values.Dequeue()
            : throw new InvalidOperationException("MockRandomSource: value queue is empty.");

    /// <inheritdoc/>
    public void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _values.Count > 0 ? _values.Dequeue() % (i + 1) : 0;
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

// ---------------------------------------------------------------------------
//  Assertion helpers
//  NOTE: internal — methods take internal engine types (GameState, EventLog)
// ---------------------------------------------------------------------------

/// <summary>
/// Domain-aware assertion helpers.  Methods that take engine-internal types
/// are marked <c>internal</c>; the test project accesses them via
/// <c>InternalsVisibleTo</c> on <c>Archetype.Engine</c>.
/// </summary>
public static class ArchAssert
{
    // ── Event log ──────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that an event with <paramref name="keyword"/> was logged, with
    /// <c>BoundArgs</c> matching all supplied expected values.  Returns the
    /// matching event.
    /// </summary>
    public static GameEvent EventLogged(
        IEnumerable<GameEvent> log,
        string keyword,
        params (string param, object value)[] args)
    {
        var events = log.Where(e => e.KeywordName == keyword).ToList();
        Assert.True(events.Count > 0, $"Expected event '{keyword}' in log but none was found.");

        foreach (var e in events)
        {
            bool match = args.All(expected =>
                e.BoundArgs.TryGetValue(expected.param, out var actual) &&
                Equals(actual, expected.value));
            if (match) return e;
        }

        var found = events.First();
        var actualStr   = string.Join(", ", found.BoundArgs.Select(kv => $"{kv.Key}={kv.Value}"));
        var expectedStr = string.Join(", ", args.Select(a => $"{a.param}={a.value}"));
        Assert.Fail($"Event '{keyword}' found but args did not match.\n  Expected: {expectedStr}\n  Actual:   {actualStr}");
        throw new InvalidOperationException(); // unreachable
    }

    /// <summary>Asserts that no event with <paramref name="keyword"/> was logged.</summary>
    public static void NoEventLogged(IEnumerable<GameEvent> log, string keyword)
    {
        Assert.False(log.Any(e => e.KeywordName == keyword),
            $"Expected no event '{keyword}' in log but one was found.");
    }

    // ── Game state (internal — GameState is internal in Archetype.Engine) ──

    /// <summary>Asserts the named accumulator on an atom equals the expected value.</summary>
    internal static void Accumulator(GameState state, AtomId atom, string name, double expected)
    {
        var actual = state.GetAtom(atom).Accumulators.TryGetValue(name, out var v) ? v : 0.0;
        Assert.Equal(expected, actual);
    }

    /// <summary>Asserts the named condition is present on the atom.</summary>
    internal static void ConditionPresent(GameState state, AtomId atom, string conditionName)
    {
        Assert.True(state.GetAtom(atom).HasCondition(conditionName),
            $"Expected condition '{conditionName}' to be present on {atom}.");
    }

    /// <summary>Asserts the named condition is absent from the atom.</summary>
    internal static void ConditionAbsent(GameState state, AtomId atom, string conditionName)
    {
        Assert.False(state.GetAtom(atom).HasCondition(conditionName),
            $"Expected condition '{conditionName}' to be absent on {atom}.");
    }

    /// <summary>Asserts the card is in the specified zone.</summary>
    internal static void InZone(GameState state, AtomId card, AtomId zone)
    {
        var actual = state.GetAtom(card).ZoneId;
        Assert.Equal(zone, actual);
    }

    /// <summary>Asserts the card is NOT in the specified zone.</summary>
    internal static void NotInZone(GameState state, AtomId card, AtomId zone)
    {
        var actual = state.GetAtom(card).ZoneId;
        Assert.NotEqual(zone, actual);
    }

    /// <summary>Asserts there is at least one active static effect on <paramref name="ownerAtom"/>.</summary>
    internal static void HasActiveEffect(GameState state, AtomId ownerAtom)
    {
        Assert.True(state.ActiveStaticEffects.Any(se => se.OwnerAtom == ownerAtom),
            $"Expected an active static effect on {ownerAtom} but none was found.");
    }

    /// <summary>Asserts there is no active static effect on <paramref name="ownerAtom"/>.</summary>
    internal static void HasNoActiveEffect(GameState state, AtomId ownerAtom)
    {
        Assert.False(state.ActiveStaticEffects.Any(se => se.OwnerAtom == ownerAtom),
            $"Expected no active static effect on {ownerAtom} but one was found.");
    }

    /// <summary>Asserts the atom has a dormant declarative effect.</summary>
    internal static void HasDormantEffect(GameState state, AtomId ownerAtom)
    {
        Assert.True(state.DormantDeclarativeEffects.Any(d => d.OwnerAtom == ownerAtom),
            $"Expected a dormant effect on {ownerAtom} but none was found.");
    }
}

// ---------------------------------------------------------------------------
//  Minimal GameDefinition factory for tests
// ---------------------------------------------------------------------------

/// <summary>
/// Builds a <see cref="GameDefinition"/> with the minimum required fields
/// for unit tests.
/// </summary>
public static class TestDefinition
{
    /// <summary>
    /// Creates a <see cref="GameDefinition"/> containing only built-in keywords
    /// (no game-creator keywords, cards, or zones).
    /// </summary>
    public static GameDefinition Minimal()
    {
        var keywords = BuiltInKeywords.All.ToDictionary(k => k.Name);
        return new GameDefinition(
            Keywords:               keywords,
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            StateBasedRules:        new List<StateBasedRule>(),
            Phases:                 new List<PhaseDefinition>(),
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>(),
            // D29: InitManifest is required; most test helpers use an empty manifest.
            InitManifest:           InitManifest.Empty,
            Id:                     "test-game");
    }

    /// <summary>Returns a new definition with the supplied keyword added.</summary>
    public static GameDefinition WithKeyword(this GameDefinition def, KeywordDefinition kw)
    {
        var updated = new Dictionary<string, KeywordDefinition>(def.Keywords) { [kw.Name] = kw };
        return def with { Keywords = updated };
    }
}

/// <summary>
/// Factory for test-scoped <see cref="Archetype.Engine.ExecutionContext"/> instances.
/// Internal because it references the internal <c>GameState</c> and <c>EventLog</c>.
/// </summary>
internal static class TestContext
{
    /// <summary>
    /// Builds an execution context for the given game state and event log.
    /// </summary>
    public static EngineContext Create(
        GameState state,
        EngineEventLog log,
        GameDefinition? definition = null,
        string activePlayer = "player1",
        IReadOnlyDictionary<string, IPlayerStrategy>? strategies = null)
    {
        return new EngineContext(
            gameState:        state,
            eventLog:         log,
            bindings:         new Dictionary<string, object>(),
            strategies:       strategies ?? new Dictionary<string, IPlayerStrategy>
            {
                [activePlayer] = new ScriptedPlayerStrategy(),
            },
            randomSource:     new MockRandomSource(),
            definition:       definition ?? TestDefinition.Minimal(),
            activePlayerName: activePlayer);
    }
}
