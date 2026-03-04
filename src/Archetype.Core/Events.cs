namespace Archetype.Core;

/// <summary>
/// A first-class runtime value that wraps a <see cref="GameEvent"/> reference.
/// Stored in trigger-fired block bindings under the reserved name
/// <c>trigger_event</c> so blocks can extract individual args via
/// the <c>event-arg</c> built-in (D8).
/// </summary>
public sealed class EventRef
{
    /// <summary>The wrapped event.</summary>
    public GameEvent Event { get; }

    /// <summary>Wraps <paramref name="ev"/> as an <see cref="EventRef"/>.</summary>
    public EventRef(GameEvent ev) => Event = ev;
}

/// <summary>
/// A single node in the event tree produced by the execution interpreter.
/// Every mutation keyword invocation produces a <c>GameEvent</c>; composite
/// keywords nest their children in <see cref="Children"/>.
/// <para>
/// <see cref="SequenceNumber"/> is assigned on finalisation (when the node
/// is popped off the parent-event stack), so it reflects completion order.
/// </para>
/// </summary>
public sealed class GameEvent
{
    /// <summary>
    /// Monotonic completion-order counter.  Assigned when the event is
    /// finalised; 0 before finalisation.
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>The keyword that produced this event.</summary>
    public string KeywordName { get; init; } = string.Empty;

    /// <summary>
    /// Arguments as evaluated at call time.  Keys are declared parameter
    /// names; values are the resolved runtime objects (e.g. <see cref="AtomId"/>,
    /// <see cref="double"/>, <see cref="bool"/>).
    /// </summary>
    public IReadOnlyDictionary<string, object> BoundArgs { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Events produced by internally invoked mutation keywords, in completion
    /// order.  Primitives have an empty children list.
    /// </summary>
    public IReadOnlyList<GameEvent> Children => _children;

    private readonly List<GameEvent> _children = new();

    /// <summary>
    /// Appends a child event.  Called by the execution interpreter to build
    /// the nested event tree for composite keyword invocations.
    /// </summary>
    public void AddChild(GameEvent child) => _children.Add(child);

    /// <summary>
    /// Enumerates this event and all of its descendants in depth-first
    /// pre-order.  Use this when scanning the event tree for trigger
    /// candidates: events produced inside composite keywords are only
    /// visible as tree descendants, not as flat log entries.
    /// </summary>
    public IEnumerable<GameEvent> SelfAndDescendants()
    {
        yield return this;
        foreach (var child in _children)
            foreach (var desc in child.SelfAndDescendants())
                yield return desc;
    }
}
