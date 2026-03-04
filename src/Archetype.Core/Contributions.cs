namespace Archetype.Core;

/// <summary>
/// Opaque ID shared by modifiers, conditions, and static effects.
/// Allocated from a single monotonic counter on <c>GameState</c> so that
/// allocation order is totally unambiguous — used to determine oldest-first
/// trigger firing order (D5, D6).
/// </summary>
public readonly record struct ContributionId(long Value)
{
    /// <summary>Sentinel representing "no contribution".</summary>
    public static readonly ContributionId None = new(0);

    /// <inheritdoc/>
    public override string ToString() => $"contrib:{Value}";
}

/// <summary>Alias used when the ID refers to a static effect instance.</summary>
public readonly record struct StaticEffectId(long Value)
{
    /// <inheritdoc/>
    public override string ToString() => $"se:{Value}";
}

/// <summary>Allocates monotonic IDs used for both contributions and static effects.</summary>
public sealed class ContributionIdCounter
{
    private long _next = 1; // 0 is reserved for .None sentinels

    /// <summary>Allocates the next <see cref="ContributionId"/>.</summary>
    public ContributionId NextContribution() => new(_next++);

    /// <summary>Allocates the next <see cref="StaticEffectId"/>.</summary>
    public StaticEffectId NextStaticEffect() => new(_next++);
}

/// <summary>Identifies the source of a contribution: either an atom or a static effect.</summary>
public abstract record ContributionSource;

/// <summary>Contribution created directly by a keyword invocation on behalf of an atom.</summary>
public sealed record AtomSource(AtomId AtomId) : ContributionSource;

/// <summary>Contribution created and owned by a static effect.</summary>
public sealed record StaticEffectSource(StaticEffectId EffectId) : ContributionSource;

/// <summary>Modifier kind, controlling how contributions stack (D5).</summary>
public enum ModifierKind
{
    /// <summary>Added into the base value: <c>base + Σ additives</c>.</summary>
    Additive,
    /// <summary>Multiplied against the post-additive result: <c>post × Π multiplicatives</c>.</summary>
    Multiplicative,
}

/// <summary>
/// A single modifier contribution on an atom's property.  Multiple contributions
/// to the same property stack via the formula: <c>(base + Σ additives) × Π multiplicatives</c>.
/// </summary>
public sealed record ModifierContribution(
    ContributionId Id,
    ContributionSource Source,
    AtomId TargetAtom,
    string PropertyName,
    ModifierKind Kind,
    double Value,
    LifetimeSpec? Lifetime = null);

/// <summary>
/// A single condition contribution applied to an atom.  Presence of one or
/// more contributions means the condition is active.
/// </summary>
public sealed record ConditionContribution(
    ContributionId Id,
    ContributionSource Source,
    AtomId TargetAtom,
    string ConditionName,
    LifetimeSpec? Lifetime = null);

// ---------------------------------------------------------------------------
//  LifetimeSpec — when an effect or contribution expires (D6)
// ---------------------------------------------------------------------------

/// <summary>
/// Specifies when a static effect or contribution expires.  The conditions
/// are OR'd: any satisfied condition triggers expiry.  An empty list means
/// "permanent" — the effect never expires through normal lifetime checking.
/// </summary>
/// <param name="Conditions">
/// The expiry conditions, OR'd together.  Empty = permanent.
/// </param>
public sealed record LifetimeSpec(IReadOnlyList<LifetimeCondition> Conditions)
{
    /// <summary>A permanent lifetime (never expires via the lifetime checker).</summary>
    public static readonly LifetimeSpec Permanent = new(Array.Empty<LifetimeCondition>());

    /// <summary>Returns <c>true</c> if this lifetime never expires automatically.</summary>
    public bool IsPermanent => Conditions.Count == 0;
}

/// <summary>Base type for conditions that can terminate a lifetime.</summary>
public abstract record LifetimeCondition;

/// <summary>Expires after the given number of turns.</summary>
public sealed record TurnTimer(int Turns) : LifetimeCondition;

/// <summary>Expires after the trigger has fired the given number of times.</summary>
public sealed record TriggerCount(int Count) : LifetimeCondition;

/// <summary>
/// Expires when the given boolean expression evaluates to <c>false</c>.
/// The expression is a property-keyword subtree — no side effects, no event log.
/// </summary>
public sealed record WhileCondition(KeywordNode Expression) : LifetimeCondition;
