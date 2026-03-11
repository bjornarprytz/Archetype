namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  HostManifest — session-time append and patch layer (D29)
// ---------------------------------------------------------------------------

/// <summary>
/// An optional append-and-patch layer supplied by the host at session build
/// time.  Applied after <see cref="InitManifest"/> completes — the engine
/// creates host zones, then host cards, then applies state overrides.
/// <para>
/// <b>Append only.</b>  The host may add zones and cards on top of what
/// <see cref="InitManifest"/> provisioned, but may not remove or replace any
/// atom the definition declares.
/// </para>
/// <para>
/// Omitting <c>WithHostManifest</c> on <c>GameSessionBuilder</c> is equivalent
/// to supplying an empty <see cref="HostManifest"/> — no host atoms are added.
/// </para>
/// </summary>
public sealed record HostManifest(
    IReadOnlyList<ZoneSpec> Zones,
    IReadOnlyList<CardSpec> Cards,
    IReadOnlyList<AtomStateOverride> StateOverrides);

// ---------------------------------------------------------------------------
//  AtomStateOverride — patch mutable state on InitManifest atoms (D29)
// ---------------------------------------------------------------------------

/// <summary>
/// Patches the mutable state of a single atom provisioned by
/// <see cref="InitManifest"/>.
/// <para>
/// <b>Accumulator overrides — merge (patch):</b> each key in
/// <see cref="Accumulators"/> sets the corresponding accumulator on the target
/// atom.  Keys absent from the override are left at their manifest-provisioned
/// values; the entire accumulator map is never replaced.
/// </para>
/// <para>
/// <b>Condition overrides — append:</b> each condition name in
/// <see cref="Conditions"/> is applied additively.  Conditions already present
/// from <see cref="InitManifest"/> are unaffected; no conditions are removed.
/// </para>
/// <para>
/// May only target atoms provisioned by <see cref="InitManifest"/>.  Targeting
/// a <see cref="HostManifest"/>-added atom is a <see cref="SessionException"/>
/// at <c>GameSessionBuilder.Build()</c> time.
/// </para>
/// </summary>
public sealed record AtomStateOverride(
    OverrideTarget Target,
    IReadOnlyDictionary<string, double>? Accumulators = null,
    IReadOnlyList<string>? Conditions = null);

// ---------------------------------------------------------------------------
//  OverrideTarget — discriminated union (D29)
// ---------------------------------------------------------------------------

/// <summary>
/// Identifies which atom an <see cref="AtomStateOverride"/> targets.
/// </summary>
public abstract record OverrideTarget;

/// <summary>
/// Targets a zone whose <see cref="ZoneSpec.LocalId"/> matches
/// <see cref="LocalId"/> in <see cref="InitManifest"/> or
/// <see cref="HostManifest"/> zones.
/// </summary>
public sealed record ZoneTarget(string LocalId) : OverrideTarget;

/// <summary>
/// Targets a card whose <see cref="CardSpec.LocalId"/> matches
/// <see cref="LocalId"/> in <see cref="InitManifest"/> cards.
/// Only cards with a non-null <see cref="CardSpec.LocalId"/> can be targeted;
/// targeting a null-LocalId card is a <see cref="SessionException"/>.
/// </summary>
public sealed record CardTarget(string LocalId) : OverrideTarget;

/// <summary>
/// Targets the player atom whose registered name matches
/// <see cref="PlayerName"/>.
/// </summary>
public sealed record PlayerTarget(string PlayerName) : OverrideTarget;
