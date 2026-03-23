using Archetype.Core;

namespace Archetype.Build;

/// <summary>
/// Build-time validator that checks field-name arguments in
/// <c>modify-accumulator</c>, <c>apply-condition</c>, <c>remove-condition</c>,
/// <c>apply-modifier</c>, and <c>get-state</c> invocations against the declared
/// <see cref="StateFieldDecl"/> sets on the target atom type (D38 / §2.6).
/// <para>
/// <c>clear-accumulator</c> is not currently in <see cref="BuiltInKeywords.All"/>
/// and is therefore not listed here; any reference to it in a keyword body is
/// rejected earlier by <see cref="GameDefinitionBuilder"/> before this validator runs.
/// </para>
/// <para>
/// <b>Known limitation:</b> when the target atom argument is not statically
/// resolvable to a specific <see cref="AtomKind"/> (e.g. an untyped
/// <see cref="ParameterRef"/> inside an effect block, or a generic
/// <see cref="TypeName.Atom"/>-typed parameter), the check is silently skipped
/// to avoid false positives.
/// </para>
/// </summary>
public static class StateMapValidator
{
    // Field-name keyword sentinel names and the StateFieldType(s) each requires.
    // NOTE: "clear-accumulator" is intentionally absent — it is not currently in
    // BuiltInKeywords.All and any keyword body that invokes it would be rejected
    // by GameDefinitionBuilder.Build() before this validator is reached.
    private static readonly IReadOnlyDictionary<string, StateFieldType?> FieldKeywords =
        new Dictionary<string, StateFieldType?>
        {
            // null means "any declared type is accepted"
            ["modify-accumulator"] = StateFieldType.Number,
            ["apply-modifier"]     = StateFieldType.Number,
            ["apply-condition"]    = StateFieldType.Bool,
            ["remove-condition"]   = StateFieldType.Bool,
            ["get-state"]          = null,
        };

    // Engine-reserved session fields — implicitly declared, never required in
    // SessionStateMapDeclarations (§2.4).
    private static readonly HashSet<string> ReservedSessionFields =
        new(StringComparer.Ordinal) { "turn-number", "phase-index" };

    // Sentinel: an empty declaration set means "unconstrained" — no validation.
    private static readonly IReadOnlyDictionary<string, StateFieldType> EmptyDeclarations =
        new Dictionary<string, StateFieldType>();

    /// <summary>
    /// Walks every keyword body, card effect block, cost body, static effect
    /// block, phase init/cleanup, state-based rule body, and action rule
    /// before/after in <paramref name="definition"/>, and verifies that each
    /// field-name-using invocation references a declared field of the correct
    /// type for its target atom kind.
    /// </summary>
    /// <exception cref="DefinitionException">
    /// Thrown when a field-name invocation references a field not declared on
    /// the target atom's <see cref="StateFieldDecl"/> list.
    /// </exception>
    public static void Validate(GameDefinition definition)
    {
        // Build per-kind declaration sets for fast lookup.
        // Card and zone declarations are per-definition, so we build a unified
        // lookup keyed by definition name; zone and player follow the same pattern.
        // For kind-level validation (where we only know the kind, not the specific
        // definition name), we use the union of all declarations for that kind.
        var cardUnion   = UnionDeclarations(definition.CardDefinitions.Values
                              .Select(c => c.StateMapDeclarations));
        var zoneUnion   = UnionDeclarations(definition.ZoneDefinitions.Values
                              .Select(z => z.StateMapDeclarations));
        var playerUnion = UnionDeclarations(definition.PlayerDefinitions.Values
                              .Select(p => p.StateMapDeclarations));

        // Session: only validate when the game creator explicitly provided
        // SessionStateMapDeclarations.  When null, session is unconstrained.
        // The reserved fields are always included in the lookup set (for the
        // implicit-declaration pass-through tests), but the union count is used
        // to decide whether to enforce constraints.
        var sessionDecl = BuildSessionSet(definition.SessionStateMapDeclarations);

        // Track which kinds have been opted in to state-map validation.
        // A kind is opted in when at least one definition of that kind
        // declares explicit StateMapDeclarations (not null).
        var cardOptedIn   = definition.CardDefinitions.Values.Any(c => c.StateMapDeclarations is not null);
        var zoneOptedIn   = definition.ZoneDefinitions.Values.Any(z => z.StateMapDeclarations is not null);
        var playerOptedIn = definition.PlayerDefinitions.Values.Any(p => p.StateMapDeclarations is not null);
        var sessionOptedIn = definition.SessionStateMapDeclarations is not null;

        // Build effective declaration maps: empty map = unconstrained for that kind.
        var kindDeclarations = new Dictionary<AtomKind, IReadOnlyDictionary<string, StateFieldType>>
        {
            [AtomKind.Card]    = cardOptedIn   ? cardUnion   : EmptyDeclarations,
            [AtomKind.Zone]    = zoneOptedIn   ? zoneUnion   : EmptyDeclarations,
            [AtomKind.Player]  = playerOptedIn ? playerUnion : EmptyDeclarations,
            [AtomKind.Session] = sessionOptedIn ? sessionDecl : EmptyDeclarations,
        };

        // Validate game-creator keyword bodies.
        foreach (var kw in definition.Keywords.Values)
        {
            if (kw.IsPrimitive || kw.Body is null) continue;
            var paramTypes = BuildParamTypeMap(kw.Parameters);
            ValidateNode(kw.Body, paramTypes, kindDeclarations, $"keyword '{kw.Name}'");
        }

        // Validate card effect blocks (primary, additional, cost, static effects).
        foreach (var card in definition.CardDefinitions.Values)
        {
            ValidateBlock(card.PrimaryEffect, emptyParams: true, kindDeclarations,
                $"card '{card.Name}' primary effect");
            foreach (var extra in card.AdditionalEffects)
                ValidateBlock(extra.Body, emptyParams: true, kindDeclarations,
                    $"card '{card.Name}' effect '{extra.Name}'");
            foreach (var cost in card.Cost ?? [])
                ValidateBlock(cost.Body, emptyParams: true, kindDeclarations,
                    $"card '{card.Name}' cost");
            foreach (var se in card.StaticEffects)
            {
                if (se.StateContributionBlock is not null)
                    ValidateBlock(se.StateContributionBlock, emptyParams: true, kindDeclarations,
                        $"card '{card.Name}' static effect state contribution");
                if (se.Trigger?.FiredBlock is not null)
                    ValidateBlock(se.Trigger.FiredBlock, emptyParams: true, kindDeclarations,
                        $"card '{card.Name}' static effect trigger");
            }
        }

        // Validate phase init/cleanup blocks.
        foreach (var phase in definition.Phases)
        {
            if (phase.Init is not null)
                ValidateBlock(phase.Init, emptyParams: true, kindDeclarations,
                    $"phase '{phase.Name}' init");
            if (phase.Cleanup is not null)
                ValidateBlock(phase.Cleanup, emptyParams: true, kindDeclarations,
                    $"phase '{phase.Name}' cleanup");
        }

        // Validate state-based rule bodies.
        foreach (var rule in definition.StateBasedRules)
        {
            ValidateBlock(rule.Body, emptyParams: true, kindDeclarations,
                $"state-based rule '{rule.Name}'");
        }

        // Validate action rule before/after blocks.
        foreach (var (actionType, rules) in definition.ActionRules)
        {
            foreach (var rule in rules)
            {
                if (rule.Before is not null)
                    ValidateBlock(rule.Before, emptyParams: true, kindDeclarations,
                        $"action rule '{actionType}' before");
                if (rule.After is not null)
                    ValidateBlock(rule.After, emptyParams: true, kindDeclarations,
                        $"action rule '{actionType}' after");
            }
        }
    }

    // -----------------------------------------------------------------------
    //  Block / node traversal
    // -----------------------------------------------------------------------

    private static void ValidateBlock(
        EffectBlockDef block,
        bool emptyParams,
        IReadOnlyDictionary<AtomKind, IReadOnlyDictionary<string, StateFieldType>> kindDeclarations,
        string context)
    {
        // Effect blocks have no declared parameters; only `session()` invocations
        // allow static atom kind resolution.
        var paramTypes = emptyParams
            ? new Dictionary<string, TypeName>()
            : throw new InvalidOperationException("emptyParams must be true in this overload");

        foreach (var step in block.Steps)
        {
            // Check if the step itself is a field-name keyword.
            CheckInvocationByName(
                step.KeywordName,
                step.ArgNodes,
                paramTypes,
                kindDeclarations,
                context);

            // Recurse into argument nodes.
            foreach (var arg in step.ArgNodes)
                ValidateNode(arg, paramTypes, kindDeclarations, context);
        }
    }

    private static void ValidateNode(
        KeywordNode node,
        IReadOnlyDictionary<string, TypeName> paramTypes,
        IReadOnlyDictionary<AtomKind, IReadOnlyDictionary<string, StateFieldType>> kindDeclarations,
        string context)
    {
        if (node is not Invocation inv) return;

        CheckInvocationByName(inv.KeywordName, inv.Args, paramTypes, kindDeclarations, context);

        foreach (var arg in inv.Args)
            ValidateNode(arg, paramTypes, kindDeclarations, context);
    }

    private static void CheckInvocationByName(
        string keywordName,
        IReadOnlyList<KeywordNode> args,
        IReadOnlyDictionary<string, TypeName> paramTypes,
        IReadOnlyDictionary<AtomKind, IReadOnlyDictionary<string, StateFieldType>> kindDeclarations,
        string context)
    {
        if (!FieldKeywords.TryGetValue(keywordName, out var requiredType)) return;

        // arg[0] = atom, arg[1] = field-name string
        if (args.Count < 2) return;

        // Resolve the static atom kind of the first argument.
        var atomKind = ResolveAtomKind(args[0], paramTypes);
        if (atomKind is null) return; // Cannot statically determine → skip.

        // Resolve the field-name string literal (second argument).
        if (args[1] is not Literal { Value: string fieldName }) return; // Not a literal → skip.

        // Engine-reserved session fields are implicitly valid for get-state on session.
        if (atomKind == AtomKind.Session && ReservedSessionFields.Contains(fieldName)) return;

        var declarations = kindDeclarations[atomKind.Value];

        // If no declarations exist for this atom kind (null across all definitions),
        // the kind is unconstrained — skip validation for backward compatibility.
        // Validation only applies when the game creator has opted in by declaring
        // at least one field on the target atom kind.
        if (declarations.Count == 0) return;

        if (!declarations.TryGetValue(fieldName, out var actualType))
        {
            throw new DefinitionException(
                $"In {context}: keyword '{keywordName}' references undeclared " +
                $"field '{fieldName}' on {atomKind.Value} atom. " +
                $"Declare it in the {atomKind.Value} definition's StateMapDeclarations.");
        }

        // get-state accepts any declared field type; others require a specific type.
        if (requiredType is not null && actualType != requiredType.Value)
        {
            throw new DefinitionException(
                $"In {context}: keyword '{keywordName}' requires a " +
                $"{requiredType.Value} field but '{fieldName}' on {atomKind.Value} " +
                $"atom is declared as {actualType}.");
        }
    }

    // -----------------------------------------------------------------------
    //  Static atom kind resolution
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resolves the static <see cref="AtomKind"/> of a keyword node argument.
    /// Returns <c>null</c> when the kind cannot be determined statically.
    /// </summary>
    private static AtomKind? ResolveAtomKind(
        KeywordNode node,
        IReadOnlyDictionary<string, TypeName> paramTypes)
    {
        return node switch
        {
            // A parameter reference is resolvable if its declared type maps to a specific kind.
            ParameterRef pr when paramTypes.TryGetValue(pr.Name, out var typeName)
                => TypeNameToAtomKind(typeName),

            // session() always resolves to Session.
            Invocation { KeywordName: "session", Args.Length: 0 }
                => AtomKind.Session,

            // All other invocations are not statically resolvable.
            _ => null,
        };
    }

    private static AtomKind? TypeNameToAtomKind(TypeName typeName) => typeName switch
    {
        TypeName.Card    => AtomKind.Card,
        TypeName.Zone    => AtomKind.Zone,
        TypeName.Player  => AtomKind.Player,
        TypeName.Session => AtomKind.Session,
        // TypeName.Atom is the generic untyped atom — not statically resolvable.
        _ => null,
    };

    // -----------------------------------------------------------------------
    //  Declaration set helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a map from field name → StateFieldType from a parameter type dictionary.
    /// </summary>
    private static IReadOnlyDictionary<string, TypeName> BuildParamTypeMap(
        IEnumerable<ParameterDecl> parameters)
    {
        var map = new Dictionary<string, TypeName>(StringComparer.Ordinal);
        foreach (var p in parameters)
            map[p.Name] = p.Type;
        return map;
    }

    /// <summary>
    /// Unions multiple StateMapDeclarations lists into a single name → type map.
    /// Conflicting types for the same name raise <see cref="InvalidOperationException"/>
    /// (should have been caught earlier by Build()).
    /// </summary>
    private static IReadOnlyDictionary<string, StateFieldType> UnionDeclarations(
        IEnumerable<IReadOnlyList<StateFieldDecl>?> sources)
    {
        var result = new Dictionary<string, StateFieldType>(StringComparer.Ordinal);
        foreach (var list in sources)
        {
            if (list is null) continue;
            foreach (var decl in list)
            {
                if (result.TryGetValue(decl.Name, out var existing))
                {
                    if (existing != decl.FieldType)
                        throw new InvalidOperationException(
                            $"Conflicting StateFieldType for field '{decl.Name}': " +
                            $"declared as both {existing} and {decl.FieldType}.");
                    // Same name + same type → deduplication is fine.
                }
                else
                {
                    result[decl.Name] = decl.FieldType;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Builds the session declaration set, including implicit engine-reserved
    /// fields (<c>turn-number</c>, <c>phase-index</c>).
    /// </summary>
    private static IReadOnlyDictionary<string, StateFieldType> BuildSessionSet(
        IReadOnlyList<StateFieldDecl>? sessionDeclarations)
    {
        var result = new Dictionary<string, StateFieldType>(StringComparer.Ordinal)
        {
            // Engine-reserved fields are always present.
            ["turn-number"] = StateFieldType.Number,
            ["phase-index"] = StateFieldType.Number,
        };

        if (sessionDeclarations is null) return result;

        foreach (var decl in sessionDeclarations)
        {
            if (result.TryGetValue(decl.Name, out var existing))
            {
                if (existing != decl.FieldType)
                    throw new DefinitionException(
                        $"Session field '{decl.Name}' is reserved by the engine and " +
                        $"cannot be redeclared with type {decl.FieldType}.");
                // Same type → no conflict.
            }
            else
            {
                result[decl.Name] = decl.FieldType;
            }
        }
        return result;
    }
}
