using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// The append-only event log for a game session.  Provides queryable scopes
/// for in-flight and finalised events, aligned with the domain model scope
/// hierarchy (D4).
/// <para>
/// Lives in <c>Archetype.Engine</c> because it holds live execution state.
/// <see cref="GameEvent"/> nodes (the data) live in <c>Archetype.Core</c>
/// and are exposed to game creators via <see cref="GameResult.FinalLog"/>.
/// </para>
/// </summary>
internal sealed class EventLog
{
    // Finalised events committed to the permanent log after all scopes close.
    private readonly List<GameEvent> _game = new();

    // Live scope accumulators — active during execution.
    private readonly List<GameEvent> _turnAccum   = new();
    private readonly List<GameEvent> _actionAccum = new();
    private readonly List<GameEvent> _blockAccum  = new();

    // When non-empty, child events are nested under the top-of-stack composite
    // rather than appended flat to _blockAccum.  Pushed/popped by
    // EvaluateComposite to implement the parent-event tree (D4).
    private readonly Stack<GameEvent> _compositeStack = new();

    private long _seqNext = 1;

    // -----------------------------------------------------------------------
    //  Scope queries (D4)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Top-level events in the current effect block (live, in-progress).
    /// Does not include events nested inside composite keyword invocations;
    /// those appear as children of their composite wrapper events.
    /// </summary>
    public IReadOnlyList<GameEvent> ThisBlock => _blockAccum;

    /// <summary>
    /// All events in the current action scope (current block included),
    /// recursively traversing composite child events so triggers can find
    /// events produced inside composite keywords.
    /// </summary>
    public IEnumerable<GameEvent> ThisAction =>
        _actionAccum.Concat(_blockAccum)
                    .SelectMany(e => e.SelfAndDescendants());

    /// <summary>
    /// All events in the current turn scope (current action and block
    /// included), recursively traversed.
    /// </summary>
    public IEnumerable<GameEvent> ThisTurn =>
        _turnAccum.Concat(_actionAccum).Concat(_blockAccum)
                  .SelectMany(e => e.SelfAndDescendants());

    /// <summary>
    /// All events in the game, including all live scopes, recursively
    /// traversed.  Triggers and queries should use this to find events
    /// produced inside composite keyword invocations.
    /// </summary>
    public IEnumerable<GameEvent> ThisGame =>
        _game.Concat(_turnAccum).Concat(_actionAccum).Concat(_blockAccum)
             .SelectMany(e => e.SelfAndDescendants());

    /// <summary>All finalised events (completed turns only).</summary>
    public IReadOnlyList<GameEvent> Finalised => _game;

    // -----------------------------------------------------------------------
    //  Scope lifecycle
    // -----------------------------------------------------------------------

    /// <summary>Opens a new block scope.  Must be paired with <see cref="CloseBlock"/>.</summary>
    public void OpenBlock()  => _blockAccum.Clear();

    /// <summary>
    /// Closes the block scope: merges block events into the action accumulator.
    /// Assigns sequence numbers to any events that haven't been stamped yet.
    /// </summary>
    public void CloseBlock()
    {
        foreach (var e in _blockAccum)
            EnsureSequenceNumber(e);
        _actionAccum.AddRange(_blockAccum);
        _blockAccum.Clear();
    }

    /// <summary>Opens a new action scope.</summary>
    public void OpenAction() => _actionAccum.Clear();

    /// <summary>Closes the action scope: merges into the turn accumulator.</summary>
    public void CloseAction()
    {
        _turnAccum.AddRange(_actionAccum);
        _actionAccum.Clear();
    }

    /// <summary>Opens a new turn scope.</summary>
    public void OpenTurn() => _turnAccum.Clear();

    /// <summary>Closes the turn scope: commits events to the permanent log.</summary>
    public void CloseTurn()
    {
        _game.AddRange(_turnAccum);
        _turnAccum.Clear();
    }

    // -----------------------------------------------------------------------
    //  Composite parent stack (D4)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Pushes a composite keyword event onto the parent stack.  While any
    /// event is on the stack, subsequent <see cref="Append"/> calls nest
    /// their events as children of the top-of-stack event rather than
    /// appending to the flat block accumulator.
    /// </summary>
    public void PushCompositeParent(GameEvent composite) =>
        _compositeStack.Push(composite);

    /// <summary>
    /// Pops the top composite event off the parent stack.  Must be called
    /// after <see cref="PushCompositeParent"/> when composite body
    /// evaluation is complete.
    /// </summary>
    public GameEvent PopCompositeParent()
    {
        if (_compositeStack.Count == 0)
            throw new InvalidOperationException(
                "EventLog composite parent stack underflow — PopCompositeParent called with empty stack.");
        return _compositeStack.Pop();
    }

    // -----------------------------------------------------------------------
    //  Event appending
    // -----------------------------------------------------------------------

    /// <summary>
    /// Appends a fully constructed event to the current scope.
    /// If a composite parent is on the stack, the event is nested as a
    /// child of that composite; otherwise it is added flat to the current
    /// block accumulator.  A sequence number is assigned in both cases.
    /// </summary>
    public void Append(GameEvent ev)
    {
        ev.SequenceNumber = _seqNext++;

        if (_compositeStack.Count > 0)
            // Nest under the innermost composite — avoids duplicating the
            // event in the flat block accumulator.
            _compositeStack.Peek().AddChild(ev);
        else
            _blockAccum.Add(ev);
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private void EnsureSequenceNumber(GameEvent ev)
    {
        // Only composite events might arrive at CloseBlock without a sequence
        // number if they were stamped before their children (shouldn't happen
        // with the current push/pop approach, but guard defensively).
        if (ev.SequenceNumber == 0)
            ev.SequenceNumber = _seqNext++;
    }
}
