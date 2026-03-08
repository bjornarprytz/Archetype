using System.Text.Json;
using System.Text.Json.Serialization;
using Archetype.Core;

namespace Archetype.Engine;

// ---------------------------------------------------------------------------
//  GameStateSnapshotSerializer — JSON round-trip for GameStateSnapshot (D17)
// ---------------------------------------------------------------------------

/// <summary>
/// Serializes and deserializes <see cref="GameStateSnapshot"/> to/from JSON
/// using <c>System.Text.Json</c>.
/// <para>
/// <b>Key design choices:</b>
/// <list type="bullet">
/// <item><see cref="GameEvent.BoundArgs"/> is <c>Dictionary&lt;string, object&gt;</c>
///   at runtime; at the serialization boundary it is converted to
///   <c>Dictionary&lt;string, BoundValue&gt;</c> (the discriminated union
///   defined in <c>Snapshot.cs</c>) and back.  This is handled by
///   <see cref="GameEventDto"/>.</item>
/// <item><see cref="EventRefValue.SequenceNumber"/> stores only the sequence
///   number; on load the full <see cref="EventRef"/> is reconstructed by
///   looking up the event in the snapshot's <see cref="GameStateSnapshot.FinalizedLog"/>.</item>
/// <item>The complete event tree (including <see cref="GameEvent.Children"/>)
///   is serialized recursively via <see cref="GameEventDto.Children"/>.</item>
/// </list>
/// </para>
/// </summary>
public static class GameStateSnapshotSerializer
{
    // -----------------------------------------------------------------------
    //  JSON options — shared, lazily created
    // -----------------------------------------------------------------------

    private static readonly JsonSerializerOptions _options = BuildOptions();

    private static JsonSerializerOptions BuildOptions()
    {
        var opts = new JsonSerializerOptions
        {
            WriteIndented         = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy  = null,  // PascalCase — matches C# record properties
        };
        opts.Converters.Add(new AtomIdConverter());
        opts.Converters.Add(new ContributionIdConverter());
        opts.Converters.Add(new StaticEffectIdConverter());
        return opts;
    }

    // -----------------------------------------------------------------------
    //  Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Serializes <paramref name="snapshot"/> to a compact JSON string.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>A JSON string that can be stored and later passed to <see cref="Deserialize"/>.</returns>
    public static string Serialize(GameStateSnapshot snapshot)
    {
        var dto = ToDto(snapshot);
        return JsonSerializer.Serialize(dto, _options);
    }

    /// <summary>
    /// Deserializes a <see cref="GameStateSnapshot"/> from a JSON string
    /// previously produced by <see cref="Serialize"/>.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The reconstructed snapshot.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is malformed or the schema version is unsupported.</exception>
    public static GameStateSnapshot Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<GameStateSnapshotDto>(json, _options)
            ?? throw new JsonException("GameStateSnapshotSerializer: deserialization returned null.");

        return FromDto(dto);
    }

    // -----------------------------------------------------------------------
    //  Snapshot → DTO (serialize direction)
    // -----------------------------------------------------------------------

    private static GameStateSnapshotDto ToDto(GameStateSnapshot s)
    {
        // Convert GameEvents (BoundArgs: object → BoundValue)
        var log = s.FinalizedLog.Select(ToEventDto).ToList();

        return new GameStateSnapshotDto(
            Version:             s.Version,
            GameDefinitionId:    s.GameDefinitionId,
            TurnNumber:          s.TurnNumber,
            NextAtomId:          s.NextAtomId,
            NextContributionId:  s.NextContributionId,
            SessionAtomId:       s.SessionAtomId,
            Atoms:               s.Atoms,
            Contributions:       s.Contributions,
            ActiveStaticEffects: s.ActiveStaticEffects,
            DormantEffects:      s.DormantEffects,
            PlayerNames:         s.PlayerNames,
            FinalizedLog:        log,
            Rng:                 s.Rng);
    }

    private static GameEventDto ToEventDto(GameEvent ev)
    {
        var boundArgs = ev.BoundArgs.ToDictionary(
            kv => kv.Key,
            kv => ObjectToBoundValue(kv.Value),
            StringComparer.Ordinal);

        var children = ev.Children.Select(ToEventDto).ToList();

        return new GameEventDto(
            ev.SequenceNumber,
            ev.KeywordName,
            boundArgs,
            children);
    }

    /// <summary>
    /// Converts a runtime <c>object</c> from <see cref="GameEvent.BoundArgs"/>
    /// to a <see cref="BoundValue"/> for JSON serialization.
    /// </summary>
    private static BoundValue ObjectToBoundValue(object value) => value switch
    {
        double d              => new NumberValue(d),
        int    i              => new NumberValue(i),
        long   l              => new NumberValue(l),
        bool   b              => new BoolValue(b),
        string s              => new StringValue(s),
        AtomId a              => new AtomIdValue(a.Value),
        ContributionId c      => new ContribIdValue(c.Value),
        EventRef er           => new EventRefValue(er.Event.SequenceNumber),
        IReadOnlyList<AtomId> col =>
            new CollectionValue(col.Select(id => (BoundValue)new AtomIdValue(id.Value)).ToList()),
        _ => throw new InvalidOperationException(
                 $"GameStateSnapshotSerializer: cannot convert BoundArgs value of type " +
                 $"'{value?.GetType().Name ?? "null"}' to BoundValue.")
    };

    // -----------------------------------------------------------------------
    //  DTO → Snapshot (deserialize direction)
    // -----------------------------------------------------------------------

    private static GameStateSnapshot FromDto(GameStateSnapshotDto dto)
    {
        if (dto.Version != GameStateSnapshot.CurrentVersion)
            throw new JsonException(
                $"GameStateSnapshotSerializer: unsupported snapshot version {dto.Version}. " +
                $"Expected {GameStateSnapshot.CurrentVersion}.");

        // Reconstruct the event tree first (children use AddChild)
        var log = dto.FinalizedLog.Select(FromEventDto).ToList();

        // Build a lookup by sequence number for EventRefValue resolution.
        var eventIndex = log
            .SelectMany(e => e.SelfAndDescendants())
            .ToDictionary(e => e.SequenceNumber);

        // Re-hydrate BoundArgs from BoundValue now that we have the event index.
        foreach (var ev in log.SelectMany(e => e.SelfAndDescendants()))
            RehydrateBoundArgs(ev, eventIndex);

        return new GameStateSnapshot(
            Version:             dto.Version,
            GameDefinitionId:    dto.GameDefinitionId,
            TurnNumber:          dto.TurnNumber,
            NextAtomId:          dto.NextAtomId,
            NextContributionId:  dto.NextContributionId,
            SessionAtomId:       dto.SessionAtomId,
            Atoms:               dto.Atoms,
            Contributions:       dto.Contributions,
            ActiveStaticEffects: dto.ActiveStaticEffects,
            DormantEffects:      dto.DormantEffects,
            PlayerNames:         dto.PlayerNames,
            FinalizedLog:        log,
            Rng:                 dto.Rng);
    }

    private static GameEvent FromEventDto(GameEventDto dto)
    {
        // BoundArgs will be re-hydrated from the DTO's typed values after
        // the whole log is reconstructed (so EventRef lookups work).
        // For now, store a temporary placeholder: the BoundValue dict itself
        // as a tagged wrapper. We'll fix it up in RehydrateBoundArgs.
        var ev = new GameEvent
        {
            SequenceNumber = dto.SequenceNumber,
            KeywordName    = dto.KeywordName,
            // Temporarily store the BoundValue dict as the args — will be replaced.
            BoundArgs      = dto.BoundArgs.ToDictionary(
                kv => kv.Key,
                kv => (object)kv.Value,   // Box BoundValue as object for now
                StringComparer.Ordinal),
        };

        foreach (var child in dto.Children)
            ev.AddChild(FromEventDto(child));

        return ev;
    }

    /// <summary>
    /// Re-hydrates <see cref="GameEvent.BoundArgs"/> by converting <see cref="BoundValue"/>
    /// objects (stored temporarily as <c>object</c>) back to runtime types.
    /// <see cref="EventRefValue"/> is resolved via <paramref name="eventIndex"/>.
    /// </summary>
    private static void RehydrateBoundArgs(GameEvent ev, Dictionary<long, GameEvent> eventIndex)
    {
        // GameEvent.BoundArgs has an init setter; we need to replace the dictionary.
        // Since BoundArgs is IReadOnlyDictionary backed by a new Dictionary in init,
        // we exploit the fact that GameEvent's BoundArgs property has 'init' semantics.
        // We re-use the same pattern as the constructor by creating a new Dictionary
        // and assigning it.  GameEvent.BoundArgs is declared as:
        //   public IReadOnlyDictionary<string, object> BoundArgs { get; init; }
        // In C# 9+ record-style init properties, they are writable in the object
        // initializer but not afterwards.  We work around this by using the
        // RehydratedBoundArgs helper on GameEvent.
        //
        // Since GameEvent is a plain class (not record) and BoundArgs has { get; init; },
        // we cannot re-set it after construction.  Instead, FromEventDto returns events
        // with BoundArgs already set to a mutable Dictionary<string, object>; this method
        // casts that backing dictionary and replaces each entry.

        if (ev.BoundArgs is not Dictionary<string, object> mutable)
        {
            // BoundArgs was set to a ReadOnlyDictionary or similar — recreate it.
            // This shouldn't happen given FromEventDto creates a plain Dictionary.
            return;
        }

        var keys = mutable.Keys.ToList();
        foreach (var key in keys)
        {
            if (mutable[key] is BoundValue bv)
                mutable[key] = BoundValueToObject(bv, eventIndex);
        }
    }

    /// <summary>
    /// Converts a <see cref="BoundValue"/> back to the runtime <c>object</c>
    /// expected by the engine.
    /// </summary>
    private static object BoundValueToObject(BoundValue bv, Dictionary<long, GameEvent> eventIndex) => bv switch
    {
        NumberValue   n => (object)n.Value,
        BoolValue     b => b.Value,
        StringValue   s => s.Value,
        AtomIdValue   a => new AtomId(a.Id),
        ContribIdValue c => new ContributionId(c.Id),
        CollectionValue col =>
            (IReadOnlyList<AtomId>)col.Items
                .OfType<AtomIdValue>()
                .Select(a => new AtomId(a.Id))
                .ToList(),
        EventRefValue er =>
            eventIndex.TryGetValue(er.SequenceNumber, out var found)
                ? new EventRef(found)
                : throw new JsonException(
                    $"GameStateSnapshotSerializer: EventRefValue references unknown " +
                    $"sequence number {er.SequenceNumber}."),
        _ => throw new JsonException(
                 $"GameStateSnapshotSerializer: unknown BoundValue subtype '{bv.GetType().Name}'.")
    };
}

// ---------------------------------------------------------------------------
//  GameStateSnapshotDto — full snapshot with typed event log (serialize-side)
// ---------------------------------------------------------------------------

/// <summary>
/// JSON-serializable mirror of <see cref="GameStateSnapshot"/>.
/// Identical except that <c>FinalizedLog</c> uses <see cref="GameEventDto"/>
/// (which has <c>Dictionary&lt;string, BoundValue&gt;</c> instead of
/// <c>Dictionary&lt;string, object&gt;</c>).
/// </summary>
internal sealed record GameStateSnapshotDto(
    int                                    Version,
    string                                 GameDefinitionId,
    int                                    TurnNumber,
    long                                   NextAtomId,
    long                                   NextContributionId,
    AtomId                                 SessionAtomId,
    IReadOnlyList<AtomSnapshotData>        Atoms,
    IReadOnlyList<ContributionSnapshot>    Contributions,
    IReadOnlyList<StaticEffectSnapshot>    ActiveStaticEffects,
    IReadOnlyList<DormantEffectSnapshot>   DormantEffects,
    IReadOnlyDictionary<long, string>      PlayerNames,
    IReadOnlyList<GameEventDto>            FinalizedLog,
    RngSnapshot                            Rng);

// ---------------------------------------------------------------------------
//  GameEventDto — serializable GameEvent with BoundValue args
// ---------------------------------------------------------------------------

/// <summary>
/// JSON-serializable representation of <see cref="GameEvent"/>.
/// Uses <see cref="BoundValue"/> for <c>BoundArgs</c> values instead of
/// raw <c>object</c>, enabling type-safe JSON round-trips.
/// </summary>
internal sealed record GameEventDto(
    long                              SequenceNumber,
    string                            KeywordName,
    IReadOnlyDictionary<string, BoundValue> BoundArgs,
    IReadOnlyList<GameEventDto>       Children);

// ---------------------------------------------------------------------------
//  Custom JSON converters for struct IDs
// ---------------------------------------------------------------------------

/// <summary>
/// Serializes <see cref="AtomId"/> as a plain JSON number rather than an object.
/// </summary>
internal sealed class AtomIdConverter : JsonConverter<AtomId>
{
    public override AtomId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, AtomId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

/// <summary>
/// Serializes <see cref="ContributionId"/> as a plain JSON number.
/// </summary>
internal sealed class ContributionIdConverter : JsonConverter<ContributionId>
{
    public override ContributionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, ContributionId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

/// <summary>
/// Serializes <see cref="StaticEffectId"/> as a plain JSON number.
/// </summary>
internal sealed class StaticEffectIdConverter : JsonConverter<StaticEffectId>
{
    public override StaticEffectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, StaticEffectId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}
