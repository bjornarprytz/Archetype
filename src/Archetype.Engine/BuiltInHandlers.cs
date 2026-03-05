using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Registers all built-in keyword implementations into the
/// <see cref="MutationDispatch"/> and <see cref="PropertyDispatch"/> tables.
/// <para>
/// Called once at <see cref="BlockExecutor"/> construction time.  A startup
/// assertion checks that every name in <see cref="BuiltInKeywords.All"/>
/// has a registered implementation and no extra names are present (D15
/// sync invariant).
/// </para>
/// </summary>
internal static class BuiltInHandlers
{
    // -----------------------------------------------------------------------
    //  Registration entry point (Task 5.3 — registers move-card)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Registers all built-in handlers into <paramref name="mutations"/> and
    /// <paramref name="properties"/>.
    /// </summary>
    public static void Register(MutationDispatch mutations, PropertyDispatch properties)
    {
        // ── Mutation primitives ──────────────────────────────────────────

        mutations.Register("modify-accumulator", ModifyAccumulator);
        mutations.Register("apply-modifier",     ApplyModifier);
        mutations.Register("remove-modifier",    RemoveModifier);
        mutations.Register("apply-condition",    ApplyCondition);
        mutations.Register("remove-condition",   RemoveCondition);
        mutations.Register("create-card",        CreateCard);
        mutations.Register("copy-card",          CopyCard);
        mutations.Register("create-zone",        CreateZone);
        mutations.Register("move-card",          MoveCard);
        mutations.Register("declare-winner",     DeclareWinner);
        mutations.Register("declare-draw",       DeclareDraw);

        // ── Property (read) primitives ───────────────────────────────────

        properties.Register("get-state",     GetState,     GetStatePure);
        properties.Register("get-property",  GetProperty,  GetPropertyPure);
        properties.Register("in-zone",       InZone,       InZonePure);
        properties.Register("has-condition", HasCondition, HasConditionPure);
        properties.Register("owner-of",      OwnerOf,      OwnerOfPure);
        properties.Register("session",       Session,      SessionPure);

        properties.Register("add",          Arith((a, b) => a + b), ArithPure((a, b) => a + b));
        properties.Register("subtract",     Arith((a, b) => a - b), ArithPure((a, b) => a - b));
        properties.Register("multiply",     Arith((a, b) => a * b), ArithPure((a, b) => a * b));
        properties.Register("max",          Arith(Math.Max),         ArithPure(Math.Max));
        properties.Register("min",          Arith(Math.Min),         ArithPure(Math.Min));

        properties.Register("and",          BoolOp((a, b) => a && b), BoolOpPure((a, b) => a && b));
        properties.Register("or",           BoolOp((a, b) => a || b), BoolOpPure((a, b) => a || b));
        properties.Register("not",          NotOp,                    NotOpPure);

        properties.Register("less-than",    Compare((a, b) => a <  b), ComparePure((a, b) => a <  b));
        properties.Register("greater-than", Compare((a, b) => a >  b), ComparePure((a, b) => a >  b));
        properties.Register("at-least",     Compare((a, b) => a >= b), ComparePure((a, b) => a >= b));
        properties.Register("at-most",      Compare((a, b) => a <= b), ComparePure((a, b) => a <= b));
        properties.Register("equal-to",     Compare((a, b) => a == b), ComparePure((a, b) => a == b));

        properties.Register("random-int",    RandomInt); // no pure variant — randomness not in conditions
        properties.Register("event-arg",     EventArg, EventArgPure);
        properties.Register("player-by-name", PlayerByName, PlayerByNamePure);
    }

    // -----------------------------------------------------------------------
    //  Startup assertion — Task 5.4
    // -----------------------------------------------------------------------

    /// <summary>
    /// Asserts that the registered handlers and <see cref="BuiltInKeywords.All"/>
    /// are in sync: every built-in has a handler, and no extra names are registered.
    /// Throws <see cref="InvalidOperationException"/> on mismatch.
    /// </summary>
    public static void AssertSync(MutationDispatch mutations, PropertyDispatch properties)
    {
        var registered = mutations.RegisteredNames
            .Concat(properties.RegisteredNames)
            .ToHashSet(StringComparer.Ordinal);

        var expected = BuiltInKeywords.All
            .Select(k => k.Name)
            .ToHashSet(StringComparer.Ordinal);

        // Every expected built-in must have a handler.
        var missing = expected.Except(registered).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Missing built-in keyword handler(s): {string.Join(", ", missing)}");

        // No extra handlers beyond what BuiltInKeywords declares.
        var extra = registered.Except(expected).ToList();
        if (extra.Count > 0)
            throw new InvalidOperationException(
                $"Extra built-in keyword handler(s) not in BuiltInKeywords.All: {string.Join(", ", extra)}");
    }

    // -----------------------------------------------------------------------
    //  §9.1  Mutation primitive implementations
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>modify-accumulator(atom, name, delta)</c> — adds delta to the named
    /// accumulator and logs a <c>modify-accumulator</c> event.
    /// </summary>
    private static object? ModifyAccumulator(object[] args, ExecutionContext ctx)
    {
        var atomId = RequireAtomId(args, 0, "modify-accumulator", "atom");
        var name   = RequireString(args, 1, "modify-accumulator", "name");
        var delta  = RequireDouble(args, 2, "modify-accumulator", "delta");

        var atom = ctx.GameState.GetAtom(atomId);
        atom.Accumulators.TryGetValue(name, out var current);
        atom.Accumulators[name] = current + delta;

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "modify-accumulator",
            BoundArgs   = new Dictionary<string, object>
            {
                ["atom"]  = atomId,
                ["name"]  = name,
                ["delta"] = delta,
            },
        });

        return null;
    }

    /// <summary>
    /// <c>apply-modifier(atom, name, kind, value, lifetime)</c> — applies a
    /// modifier contribution and returns the <see cref="ContributionId"/>.
    /// </summary>
    private static object? ApplyModifier(object[] args, ExecutionContext ctx)
    {
        var atomId   = RequireAtomId(args, 0, "apply-modifier", "atom");
        var name     = RequireString(args, 1, "apply-modifier", "name");
        var kind     = (ModifierKind)(int)RequireDouble(args, 2, "apply-modifier", "kind");
        var value    = RequireDouble(args, 3, "apply-modifier", "value");
        // Lifetime arg (index 4) accepted but not tracked in this iteration.

        var id    = ctx.GameState.NextContributionId();
        var atom  = ctx.GameState.GetAtom(atomId);
        var contribution = new ModifierContribution(
            id, new AtomSource(atomId), atomId, name, kind, value);

        if (!atom.ModifierIndex.TryGetValue(name, out var list))
            atom.ModifierIndex[name] = list = new List<ModifierContribution>();
        list.Add(contribution);
        ctx.GameState.ContributionRegistry[id] = new ModifierContributionWrapper(contribution);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "apply-modifier",
            BoundArgs   = new Dictionary<string, object>
            {
                ["atom"]  = atomId,
                ["name"]  = name,
                ["kind"]  = (double)(int)kind,
                ["value"] = value,
            },
        });

        return id;
    }

    private static object? RemoveModifier(object[] args, ExecutionContext ctx)
    {
        var id = RequireContributionId(args, 0, "remove-modifier", "id");

        if (ctx.GameState.ContributionRegistry.TryGetValue(id, out var contrib))
        {
            if (contrib is ModifierContributionWrapper { Inner: var mc })
            {
                var atom = ctx.GameState.GetAtom(mc.TargetAtom);
                if (atom.ModifierIndex.TryGetValue(mc.PropertyName, out var list))
                    list.RemoveAll(m => m.Id == id);
            }
            ctx.GameState.ContributionRegistry.Remove(id);
        }

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "remove-modifier",
            BoundArgs   = new Dictionary<string, object> { ["id"] = id },
        });
        return null;
    }

    private static object? ApplyCondition(object[] args, ExecutionContext ctx)
    {
        var atomId = RequireAtomId(args, 0, "apply-condition", "atom");
        var name   = RequireString(args, 1, "apply-condition", "name");
        // Lifetime (index 2) accepted but not tracked in this iteration.

        var id   = ctx.GameState.NextContributionId();
        var atom = ctx.GameState.GetAtom(atomId);
        var contribution = new ConditionContribution(id, new AtomSource(atomId), atomId, name);

        if (!atom.ConditionIndex.TryGetValue(name, out var list))
            atom.ConditionIndex[name] = list = new List<ConditionContribution>();
        list.Add(contribution);
        ctx.GameState.ContributionRegistry[id] = new ConditionContributionWrapper(contribution);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "apply-condition",
            BoundArgs   = new Dictionary<string, object>
            {
                ["atom"] = atomId,
                ["name"] = name,
            },
        });

        return id;
    }

    private static object? RemoveCondition(object[] args, ExecutionContext ctx)
    {
        var atomId = RequireAtomId(args, 0, "remove-condition", "atom");
        var name   = RequireString(args, 1, "remove-condition", "name");
        var atom   = ctx.GameState.GetAtom(atomId);

        if (atom.ConditionIndex.TryGetValue(name, out var list))
        {
            foreach (var c in list)
                ctx.GameState.ContributionRegistry.Remove(c.Id);
            list.Clear();
        }

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "remove-condition",
            BoundArgs   = new Dictionary<string, object>
            {
                ["atom"] = atomId,
                ["name"] = name,
            },
        });
        return null;
    }

    /// <summary>
    /// <c>create-card(zone, definition-name, owner)</c> — instantiates a
    /// card atom and places it in the specified zone.  Returns the new <see cref="AtomId"/>.
    /// </summary>
    private static object? CreateCard(object[] args, ExecutionContext ctx)
    {
        var zoneId = RequireAtomOfKind(args, 0, "create-card", "zone", AtomKind.Zone, ctx.GameState);
        var defName = RequireString(args, 1, "create-card", "definition-name");
        var ownerId = RequireAtomOfKind(args, 2, "create-card", "owner", AtomKind.Player, ctx.GameState);

        var newId = ctx.GameState.CreateAtom(AtomKind.Card, ownerId: ownerId, zoneId: zoneId);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "create-card",
            BoundArgs   = new Dictionary<string, object>
            {
                ["zone"]            = zoneId,
                ["definition-name"] = defName,
                ["owner"]           = ownerId,
                ["result"]          = newId,
            },
        });

        return newId;
    }

    private static object? CopyCard(object[] args, ExecutionContext ctx)
    {
        var sourceId  = RequireAtomOfKind(args, 0, "copy-card", "source", AtomKind.Card, ctx.GameState);
        var destZone  = RequireAtomOfKind(args, 1, "copy-card", "destination-zone", AtomKind.Zone, ctx.GameState);
        var ownerId   = RequireAtomOfKind(args, 2, "copy-card", "owner", AtomKind.Player, ctx.GameState);

        var newId = ctx.GameState.CreateAtom(AtomKind.Card, ownerId: ownerId, zoneId: destZone);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "create-card", // same event type as create-card per D12
            BoundArgs   = new Dictionary<string, object>
            {
                ["source"]           = sourceId,
                ["destination-zone"] = destZone,
                ["owner"]            = ownerId,
                ["result"]           = newId,
            },
        });

        return newId;
    }

    private static object? CreateZone(object[] args, ExecutionContext ctx)
    {
        var ownerId = RequireAtomOfKind(args, 0, "create-zone", "owner", AtomKind.Player, ctx.GameState);
        var defName = RequireString(args, 1, "create-zone", "definition-name");

        var newId = ctx.GameState.CreateAtom(AtomKind.Zone, ownerId: ownerId);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "create-zone",
            BoundArgs   = new Dictionary<string, object>
            {
                ["owner"]           = ownerId,
                ["definition-name"] = defName,
                ["result"]          = newId,
            },
        });

        return newId;
    }

    // -----------------------------------------------------------------------
    //  §9.1  move-card — Tasks 5.1 and 5.2
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>move-card(card, destination)</c> — moves an existing card to the
    /// specified zone.
    /// <para>
    /// Implementation steps (per design §Decision 1 and §Decision 2):
    /// <list type="number">
    ///   <item>Resolve both arguments; validate that <c>destination</c> is an
    ///   active zone atom (Task 5.2 — throws <see cref="EngineException"/> if not).</item>
    ///   <item>Capture <c>origin = card.ZoneId</c> <b>before</b> any mutation
    ///   (Task 5.1 — ensures the event reflects the zone at call time).</item>
    ///   <item>Update <c>card.ZoneId = destination</c>.</item>
    ///   <item>Log a <c>move-card</c> event with <c>{ card, origin, destination }</c>.</item>
    /// </list>
    /// </para>
    /// </summary>
    private static object? MoveCard(object[] args, ExecutionContext ctx)
    {
        // RequireAtomOfKind validates both that the argument is an AtomId and
        // that the atom exists with the expected kind — consistent with other
        // mutation primitives (create-card, copy-card, create-zone).
        var cardId = RequireAtomOfKind(args, 0, "move-card", "card", AtomKind.Card, ctx.GameState);
        var destId = RequireAtomOfKind(args, 1, "move-card", "destination", AtomKind.Zone, ctx.GameState);

        var cardSnapshot = ctx.GameState.GetAtom(cardId);

        // Capture origin before mutation so the event reflects call-time state (Task 5.1).
        var originId = cardSnapshot.ZoneId;

        // Mutate: update the card's zone.
        cardSnapshot.ZoneId = destId;

        // Log the event with the pre-mutation origin.
        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "move-card",
            BoundArgs   = new Dictionary<string, object>
            {
                ["card"]        = cardId,
                ["origin"]      = originId,
                ["destination"] = destId,
            },
        });

        return null; // void
    }

    // -----------------------------------------------------------------------
    //  Game outcome primitives
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>declare-winner(player)</c> — sets the game outcome to a win for the
    /// given player atom.  The session loop exits after the current
    /// post-action sequence completes.  First call wins; subsequent calls
    /// to either <c>declare-winner</c> or <c>declare-draw</c> are no-ops.
    /// </summary>
    private static object? DeclareWinner(object[] args, ExecutionContext ctx)
    {
        var playerAtomId = RequireAtomOfKind(
            args, 0, "declare-winner", "player", AtomKind.Player, ctx.GameState);

        // Resolve the player's string name from the registry built during provisioning.
        if (!ctx.GameState.TryGetPlayerName(playerAtomId, out var winnerName))
            throw new EngineException(
                $"declare-winner: atom {playerAtomId} is not a registered player name. " +
                "Ensure the player was provisioned via ManifestProvisioner or GameStateBuilder.");

        ctx.GameState.DeclareOutcome(winnerName);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "declare-winner",
            BoundArgs   = new Dictionary<string, object>
            {
                ["player"] = playerAtomId,
                ["winner"] = winnerName,
            },
        });

        return null;
    }

    /// <summary>
    /// <c>declare-draw()</c> — sets the game outcome to a draw.
    /// The session loop exits after the current post-action sequence completes.
    /// </summary>
    private static object? DeclareDraw(object[] args, ExecutionContext ctx)
    {
        ctx.GameState.DeclareOutcome(winner: null);

        ctx.EventLog.Append(new GameEvent
        {
            KeywordName = "declare-draw",
            BoundArgs   = new Dictionary<string, object>(),
        });

        return null;
    }

    // -----------------------------------------------------------------------
    //  Player lookup
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>player-by-name(name)</c> — reverse-looks up a player atom from its
    /// registered name string.  The idiomatic way to pass a player reference
    /// to <c>declare-winner</c> from a state-based rule body, where the atom
    /// ID is not known at definition time.
    /// </summary>
    private static object? PlayerByName(object[] args, ExecutionContext ctx) =>
        PlayerByNamePure(args, ctx.GameState, ctx.Bindings);

    private static object? PlayerByNamePure(
        object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var name = RequireString(args, 0, "player-by-name", "name");
        if (!state.TryGetPlayerAtomByName(name, out var atomId))
            throw new EngineException(
                $"player-by-name: no player named '{name}' is registered in this session.");
        return atomId;
    }

    // -----------------------------------------------------------------------
    //  §9.2  Property keyword implementations
    // -----------------------------------------------------------------------

    private static object? GetState(object[] args, ExecutionContext ctx) =>
        GetStatePure(args, ctx.GameState, ctx.Bindings);

    private static object? GetStatePure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var atomId = RequireAtomId(args, 0, "get-state", "atom");
        var name   = RequireString(args, 1, "get-state", "name");
        return state.GetAtom(atomId).Accumulators.TryGetValue(name, out var v) ? v : 0.0;
    }

    private static object? GetProperty(object[] args, ExecutionContext ctx) =>
        GetPropertyPure(args, ctx.GameState, ctx.Bindings);

    private static object? GetPropertyPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var atomId = RequireAtomId(args, 0, "get-property", "atom");
        var name   = RequireString(args, 1, "get-property", "name");
        return state.GetAtom(atomId).GetComputedProperty(name);
    }

    private static object? InZone(object[] args, ExecutionContext ctx) =>
        InZonePure(args, ctx.GameState, ctx.Bindings);

    private static object? InZonePure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var cardId = RequireAtomId(args, 0, "in-zone", "card");
        var zoneId = RequireAtomId(args, 1, "in-zone", "zone");
        return state.GetAtom(cardId).ZoneId == zoneId;
    }

    private static object? HasCondition(object[] args, ExecutionContext ctx) =>
        HasConditionPure(args, ctx.GameState, ctx.Bindings);

    private static object? HasConditionPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var atomId = RequireAtomId(args, 0, "has-condition", "atom");
        var name   = RequireString(args, 1, "has-condition", "name");
        return state.GetAtom(atomId).HasCondition(name);
    }

    private static object? OwnerOf(object[] args, ExecutionContext ctx) =>
        OwnerOfPure(args, ctx.GameState, ctx.Bindings);

    private static object? OwnerOfPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var atomId = RequireAtomId(args, 0, "owner-of", "atom");
        return state.GetAtom(atomId).OwnerId;
    }

    private static object? Session(object[] args, ExecutionContext ctx) =>
        SessionPure(args, ctx.GameState, ctx.Bindings);

    private static object? SessionPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _) =>
        state.SessionAtomId;

    private static object? RandomInt(object[] args, ExecutionContext ctx)
    {
        var min = (int)RequireDouble(args, 0, "random-int", "min");
        var max = (int)RequireDouble(args, 1, "random-int", "max");
        return (double)ctx.RandomSource.NextInt(min, max);
    }

    private static object? EventArg(object[] args, ExecutionContext ctx) =>
        EventArgPure(args, ctx.GameState, ctx.Bindings);

    private static object? EventArgPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _)
    {
        var ev   = RequireEventRef(args, 0, "event-arg", "event");
        var name = RequireString(args, 1, "event-arg", "name");

        if (!ev.Event.BoundArgs.TryGetValue(name, out var value))
            throw new EngineException(
                $"event-arg: argument '{name}' does not exist in the event's BoundArgs " +
                $"(event keyword: '{ev.Event.KeywordName}').");
        return value;
    }

    // -----------------------------------------------------------------------
    //  Arithmetic / Boolean factory helpers
    // -----------------------------------------------------------------------

    private static PropertyHandler Arith(Func<double, double, double> fn) =>
        (args, ctx) => fn(RequireDouble(args, 0, "arith", "a"), RequireDouble(args, 1, "arith", "b"));

    private static PurePropertyHandler ArithPure(Func<double, double, double> fn) =>
        (args, state, _) => fn(RequireDouble(args, 0, "arith", "a"), RequireDouble(args, 1, "arith", "b"));

    private static PropertyHandler BoolOp(Func<bool, bool, bool> fn) =>
        (args, ctx) => fn(RequireBool(args, 0, "bool", "a"), RequireBool(args, 1, "bool", "b"));

    private static PurePropertyHandler BoolOpPure(Func<bool, bool, bool> fn) =>
        (args, state, _) => fn(RequireBool(args, 0, "bool", "a"), RequireBool(args, 1, "bool", "b"));

    private static object? NotOp(object[] args, ExecutionContext ctx) =>
        !RequireBool(args, 0, "not", "p");

    private static object? NotOpPure(object[] args, GameState state, IReadOnlyDictionary<string, object> _) =>
        !RequireBool(args, 0, "not", "p");

    private static PropertyHandler Compare(Func<double, double, bool> fn) =>
        (args, ctx) => fn(RequireDouble(args, 0, "compare", "a"), RequireDouble(args, 1, "compare", "b"));

    private static PurePropertyHandler ComparePure(Func<double, double, bool> fn) =>
        (args, state, _) => fn(RequireDouble(args, 0, "compare", "a"), RequireDouble(args, 1, "compare", "b"));

    // -----------------------------------------------------------------------
    //  Argument coercion helpers
    // -----------------------------------------------------------------------

    private static AtomId RequireAtomId(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            AtomId id => id,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be an AtomId; got {args[idx]?.GetType().Name ?? "null"}."),
        };
    }

    private static AtomId RequireAtomOfKind(
        object[] args, int idx, string kw, string paramName,
        AtomKind expected, GameState state)
    {
        var id = RequireAtomId(args, idx, kw, paramName);
        if (!state.IsAtomOfKind(id, expected))
            throw new EngineException($"{kw}: argument '{paramName}' ({id}) must be a {expected} atom.");
        return id;
    }

    private static string RequireString(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            string s => s,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be a string; got {args[idx]?.GetType().Name ?? "null"}."),
        };
    }

    private static double RequireDouble(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            double d   => d,
            int i      => i,
            long l     => l,
            float f    => f,
            bool b     => b ? 1.0 : 0.0,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be a number; got {args[idx]?.GetType().Name ?? "null"}."),
        };
    }

    private static bool RequireBool(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            bool b   => b,
            double d => d != 0.0,
            int i    => i != 0,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be a boolean; got {args[idx]?.GetType().Name ?? "null"}."),
        };
    }

    private static EventRef RequireEventRef(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            EventRef er => er,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be an EventRef; got {args[idx]?.GetType().Name ?? "null"}."),
        };
    }

    private static ContributionId RequireContributionId(object[] args, int idx, string kw, string paramName)
    {
        if (idx >= args.Length)
            throw new EngineException($"{kw}: missing argument '{paramName}' at index {idx}.");
        return args[idx] switch
        {
            ContributionId id => id,
            _ => throw new EngineException($"{kw}: argument '{paramName}' must be a ContributionId."),
        };
    }
}

// ---------------------------------------------------------------------------
//  Contribution wrapper types for the registry
// ---------------------------------------------------------------------------

internal sealed class ModifierContributionWrapper(ModifierContribution inner) : IContribution
{
    public ContributionId Id => inner.Id;
    public AtomId TargetAtom => inner.TargetAtom;
    public ModifierContribution Inner => inner;
}

internal sealed class ConditionContributionWrapper(ConditionContribution inner) : IContribution
{
    public ContributionId Id => inner.Id;
    public AtomId TargetAtom => inner.TargetAtom;
    public ConditionContribution Inner => inner;
}
