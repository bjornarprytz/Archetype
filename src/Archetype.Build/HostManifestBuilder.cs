using Archetype.Core;

namespace Archetype.Build;

// ---------------------------------------------------------------------------
//  HostManifestBuilder — fluent construction of a HostManifest (D29)
// ---------------------------------------------------------------------------

/// <summary>
/// Fluent builder for constructing a <see cref="HostManifest"/> to be supplied
/// to <c>GameSessionBuilder.WithHostManifest(Action&lt;HostManifestBuilder&gt;)</c>.
/// <para>
/// The host may add zones and cards on top of what <c>InitManifest</c> provisioned,
/// and may patch accumulator/condition state on atoms provisioned by
/// <c>InitManifest</c>.  The host cannot remove or replace definition-declared atoms.
/// </para>
/// </summary>
public sealed class HostManifestBuilder
{
    private readonly List<ZoneSpec>         _zones          = new();
    private readonly List<CardSpec>         _cards          = new();
    private readonly List<AtomStateOverride> _stateOverrides = new();

    /// <summary>
    /// Adds a zone to the host manifest.
    /// </summary>
    /// <param name="localId">
    /// A locally-unique identifier for this zone instance.  Must not collide
    /// with any <see cref="ZoneSpec.LocalId"/> in <c>InitManifest.Zones</c>.
    /// </param>
    /// <param name="owner">Player definition name that owns this zone.</param>
    /// <param name="definition">Zone definition name from <c>GameDefinition.ZoneDefinitions</c>.</param>
    /// <param name="state">Optional callback to set initial accumulators and conditions.</param>
    public HostManifestBuilder AddZone(
        string localId,
        string owner,
        string definition,
        Action<ZoneStateBuilder>? state = null)
    {
        ZoneSpec spec;
        if (state is null)
        {
            spec = new ZoneSpec(localId, owner, definition);
        }
        else
        {
            var sb = new ZoneStateBuilder();
            state(sb);
            spec = new ZoneSpec(localId, owner, definition,
                Accumulators: sb.Accumulators,
                Conditions:   sb.Conditions);
        }
        _zones.Add(spec);
        return this;
    }

    /// <summary>
    /// Adds a card to the host manifest.
    /// </summary>
    /// <param name="owner">Player definition name that owns this card.</param>
    /// <param name="zoneLocalId">
    /// Zone LocalId (from either <c>InitManifest</c> or this <c>HostManifest</c>)
    /// where the card starts.
    /// </param>
    /// <param name="definition">Card definition name from <c>GameDefinition.CardDefinitions</c>.</param>
    /// <param name="localId">
    /// Optional.  Required only if host state overrides need to target this card.
    /// Must be unique within <c>HostManifest.Cards</c>.
    /// </param>
    /// <param name="state">Optional callback to set initial accumulators and conditions.</param>
    public HostManifestBuilder AddCard(
        string owner,
        string zoneLocalId,
        string definition,
        string? localId = null,
        Action<CardStateBuilder>? state = null)
    {
        CardSpec spec;
        if (state is null)
        {
            spec = new CardSpec(owner, zoneLocalId, definition, LocalId: localId);
        }
        else
        {
            var sb = new CardStateBuilder();
            state(sb);
            spec = new CardSpec(owner, zoneLocalId, definition,
                Accumulators: sb.Accumulators,
                Conditions:   sb.Conditions,
                LocalId:      localId);
        }
        _cards.Add(spec);
        return this;
    }

    /// <summary>
    /// Adds an accumulator/condition patch that targets a zone provisioned by
    /// <c>InitManifest</c> (keyed by <see cref="ZoneSpec.LocalId"/>).
    /// </summary>
    public HostManifestBuilder OverrideZoneState(
        string localId,
        Action<StateOverrideBuilder> configure)
    {
        var builder = new StateOverrideBuilder();
        configure(builder);
        _stateOverrides.Add(new AtomStateOverride(
            Target:       new ZoneTarget(localId),
            Accumulators: builder.Accumulators,
            Conditions:   builder.Conditions));
        return this;
    }

    /// <summary>
    /// Adds an accumulator/condition patch that targets a card provisioned by
    /// <c>InitManifest</c> (keyed by <see cref="CardSpec.LocalId"/>).
    /// The card must have a non-null <see cref="CardSpec.LocalId"/>.
    /// </summary>
    public HostManifestBuilder OverrideCardState(
        string localId,
        Action<StateOverrideBuilder> configure)
    {
        var builder = new StateOverrideBuilder();
        configure(builder);
        _stateOverrides.Add(new AtomStateOverride(
            Target:       new CardTarget(localId),
            Accumulators: builder.Accumulators,
            Conditions:   builder.Conditions));
        return this;
    }

    /// <summary>
    /// Adds an accumulator/condition patch that targets the player atom for the
    /// named player definition.
    /// </summary>
    public HostManifestBuilder OverridePlayerState(
        string playerName,
        Action<StateOverrideBuilder> configure)
    {
        var builder = new StateOverrideBuilder();
        configure(builder);
        _stateOverrides.Add(new AtomStateOverride(
            Target:       new PlayerTarget(playerName),
            Accumulators: builder.Accumulators,
            Conditions:   builder.Conditions));
        return this;
    }

    /// <summary>
    /// Builds and returns the <see cref="HostManifest"/>.
    /// </summary>
    public HostManifest Build() => new(_zones, _cards, _stateOverrides);
}

// ---------------------------------------------------------------------------
//  State builder helpers
// ---------------------------------------------------------------------------

/// <summary>
/// Builds the initial state (accumulators, conditions) for a zone in the host manifest.
/// </summary>
public sealed class ZoneStateBuilder
{
    private readonly Dictionary<string, double> _accumulators = new(StringComparer.Ordinal);
    private readonly List<string>               _conditions   = new();

    /// <summary>Sets a starting accumulator value for this zone.</summary>
    public ZoneStateBuilder SetAccumulator(string name, double value)
    {
        _accumulators[name] = value;
        return this;
    }

    /// <summary>Adds a starting condition to this zone.</summary>
    public ZoneStateBuilder AddCondition(string conditionName)
    {
        _conditions.Add(conditionName);
        return this;
    }

    internal IReadOnlyDictionary<string, double>? Accumulators =>
        _accumulators.Count > 0 ? _accumulators : null;
    internal IReadOnlyList<string>? Conditions =>
        _conditions.Count > 0 ? _conditions : null;
}

/// <summary>
/// Builds the initial state (accumulators, conditions) for a card in the host manifest.
/// </summary>
public sealed class CardStateBuilder
{
    private readonly Dictionary<string, double> _accumulators = new(StringComparer.Ordinal);
    private readonly List<string>               _conditions   = new();

    /// <summary>Sets a starting accumulator value for this card.</summary>
    public CardStateBuilder SetAccumulator(string name, double value)
    {
        _accumulators[name] = value;
        return this;
    }

    /// <summary>Adds a starting condition to this card.</summary>
    public CardStateBuilder AddCondition(string conditionName)
    {
        _conditions.Add(conditionName);
        return this;
    }

    internal IReadOnlyDictionary<string, double>? Accumulators =>
        _accumulators.Count > 0 ? _accumulators : null;
    internal IReadOnlyList<string>? Conditions =>
        _conditions.Count > 0 ? _conditions : null;
}

/// <summary>
/// Builds the state override (accumulator merge, condition append) for an
/// <see cref="AtomStateOverride"/>.
/// </summary>
public sealed class StateOverrideBuilder
{
    private readonly Dictionary<string, double> _accumulators = new(StringComparer.Ordinal);
    private readonly List<string>               _conditions   = new();

    /// <summary>
    /// Sets an accumulator override value.  Merges (patch) semantics: only the
    /// specified key is changed; other accumulators on the target atom are unaffected.
    /// </summary>
    public StateOverrideBuilder SetAccumulator(string name, double value)
    {
        _accumulators[name] = value;
        return this;
    }

    /// <summary>
    /// Appends a condition to the target atom.  The condition is applied additively;
    /// existing conditions are not removed.
    /// </summary>
    public StateOverrideBuilder AddCondition(string conditionName)
    {
        _conditions.Add(conditionName);
        return this;
    }

    internal IReadOnlyDictionary<string, double>? Accumulators =>
        _accumulators.Count > 0 ? _accumulators : null;
    internal IReadOnlyList<string>? Conditions =>
        _conditions.Count > 0 ? _conditions : null;
}
