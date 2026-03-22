namespace Archetype.Core;

/// <summary>
/// The four atom kinds the engine recognises.  Atom kind is assigned at
/// creation time and is immutable.  The type-checker uses it to validate
/// <c>AtomKindRestriction</c> on built-in keyword parameters (D2/A14).
/// </summary>
public enum AtomKind
{
    /// <summary>A card instance placed in a zone.</summary>
    Card,
    /// <summary>A zone that holds cards.</summary>
    Zone,
    /// <summary>A player participating in the session.</summary>
    Player,
    /// <summary>
    /// The singleton session atom.  Carries game-wide accumulators and
    /// conditions (e.g. turn counter).  Created automatically before the
    /// first phase.
    /// </summary>
    Session,
}

/// <summary>
/// Stable, opaque identity for any atom (card, zone, player, or session).
/// Allocated from a monotonic counter on <see cref="Archetype.Core.AtomIdCounter"/>;
/// never reused within a session.
/// </summary>
public readonly record struct AtomId(long Value)
{
    /// <summary>Sentinel representing "no atom".</summary>
    public static readonly AtomId None = new(0);

    /// <inheritdoc/>
    public override string ToString() => $"atom:{Value}";

    public static implicit operator long(AtomId id) => id.Value;
    public static implicit operator AtomId(long value) => new(value);
}

/// <summary>
/// Thread-safe allocator for <see cref="AtomId"/> values.
/// A single instance lives on <c>GameState</c>.  Because the engine is
/// single-threaded (WASM, D1), there is no locking cost.
/// </summary>
public sealed class AtomIdCounter
{
    private long _next = 1; // 0 is reserved for AtomId.None

    /// <summary>Allocates the next <see cref="AtomId"/>.</summary>
    public AtomId Next() => new(_next++);

    /// <summary>
    /// Returns the next ID that would be allocated without advancing the counter.
    /// Used by snapshot capture to record the counter state.
    /// </summary>
    public long PeekNext() => _next;

    /// <summary>
    /// Restores the counter to <paramref name="next"/>.
    /// Used by snapshot load to reproduce the exact ID sequence.
    /// </summary>
    public void Restore(long next) => _next = next;
}
