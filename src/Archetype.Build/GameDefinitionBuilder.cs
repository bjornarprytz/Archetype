using Archetype.Core;

namespace Archetype.Build;

/// <summary>
/// Fluent builder for <see cref="GameDefinition"/> instances (D32).
/// <para>
/// Game creators use this class to register all rules (keywords, zones,
/// phases, state-based rules, trigger resolution order, player definitions,
/// and <see cref="InitManifest"/>) and call <see cref="Build"/> to obtain
/// a validated, immutable <see cref="GameDefinition"/>.
/// </para>
/// <example>
/// <code>
/// var definition = new GameDefinitionBuilder()
///     .WithId("my-game")
///     .AddZone("hand", new Dictionary&lt;string, object&gt;())
///     .RegisterKeyword(new KeywordDefinition(
///         Name: "take-damage",
///         Parameters: [new ParameterDecl("target", TypeName.Card), new ParameterDecl("amount", TypeName.Number)],
///         ReturnType: TypeName.Number,
///         Description: "take-damage",
///         Body: Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Num(-1)),
///         TextTemplate: "{target} takes {amount} damage"))
///     .WithInitManifest(InitManifest.Empty)
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class GameDefinitionBuilder
{
    private string? _id;
    private readonly Dictionary<string, KeywordDefinition> _keywords = new();
    private readonly Dictionary<string, CardDefinition> _cardDefinitions = new();
    private readonly Dictionary<string, ZoneDefinition> _zoneDefinitions = new();
    private readonly List<StateBasedRule> _stateBasedRules = new();
    private readonly List<PhaseDefinition> _phases = new();
    private readonly Dictionary<string, IReadOnlyList<ActionRuleDefinition>> _actionRules = new();
    private TriggerResolutionOrder _triggerResolutionOrder = TriggerResolutionOrder.OldestFirst;
    private readonly Dictionary<string, PlayerDefinition> _playerDefinitions = new();
    private InitManifest? _initManifest;
    private IReadOnlyList<string>? _playableZoneNames;
    private IReadOnlyList<StateFieldDecl>? _sessionStateMap;
    private readonly List<AtomGroup<CardDefinition>> _cardGroups = new();
    private readonly List<AtomGroup<ZoneDefinition>> _zoneGroups = new();
    private readonly List<AtomGroup<PlayerDefinition>> _playerGroups = new();

    // -----------------------------------------------------------------------
    //  Game identity
    // -----------------------------------------------------------------------

    /// <summary>Sets the game definition ID (required by <c>GameSessionBuilder</c>).</summary>
    public GameDefinitionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Zones
    // -----------------------------------------------------------------------

    /// <summary>Registers a zone definition.</summary>
    public GameDefinitionBuilder AddZone(ZoneDefinition zone)
    {
        _zoneDefinitions[zone.Name] = zone;
        return this;
    }

    /// <summary>Registers a zone definition.</summary>
    public GameDefinitionBuilder AddZone(string name, IReadOnlyDictionary<string, object> staticProperties,
        IReadOnlyList<StateFieldDecl>? stateMapDeclarations = null)
        => AddZone(new ZoneDefinition(name, staticProperties, stateMapDeclarations));

    // -----------------------------------------------------------------------
    //  Phases
    // -----------------------------------------------------------------------

    /// <summary>Appends a phase to the turn structure.</summary>
    public GameDefinitionBuilder AddPhase(PhaseDefinition phase)
    {
        _phases.Add(phase);
        return this;
    }

    // -----------------------------------------------------------------------
    //  Keywords
    // -----------------------------------------------------------------------

    /// <summary>
    /// Registers a game-creator keyword.
    /// <para>
    /// <c>Build()</c> validates that all <see cref="ParameterRef"/> names in
    /// <see cref="KeywordDefinition.Body"/> appear in
    /// <see cref="KeywordDefinition.Parameters"/>, and that every invoked
    /// keyword name is known.
    /// </para>
    /// </summary>
    public GameDefinitionBuilder RegisterKeyword(KeywordDefinition keyword)
    {
        _keywords[keyword.Name] = keyword;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Card definitions
    // -----------------------------------------------------------------------

    /// <summary>Registers a card definition.</summary>
    public GameDefinitionBuilder AddCard(CardDefinition card)
    {
        _cardDefinitions[card.Name] = card;
        return this;
    }

    // -----------------------------------------------------------------------
    //  State-based rules
    // -----------------------------------------------------------------------

    /// <summary>Appends a state-based rule (evaluated in registration order).</summary>
    public GameDefinitionBuilder AddStateBasedRule(StateBasedRule rule)
    {
        _stateBasedRules.Add(rule);
        return this;
    }

    // -----------------------------------------------------------------------
    //  Action rules
    // -----------------------------------------------------------------------

    /// <summary>Sets the action rules for a named action type.</summary>
    public GameDefinitionBuilder SetActionRules(string actionType, IReadOnlyList<ActionRuleDefinition> rules)
    {
        _actionRules[actionType] = rules;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Trigger resolution order
    // -----------------------------------------------------------------------

    /// <summary>Sets the trigger resolution order (default: <see cref="TriggerResolutionOrder.OldestFirst"/>).</summary>
    public GameDefinitionBuilder WithTriggerResolutionOrder(TriggerResolutionOrder order)
    {
        _triggerResolutionOrder = order;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Player definitions
    // -----------------------------------------------------------------------

    /// <summary>Registers a player definition.</summary>
    public GameDefinitionBuilder AddPlayer(string name, PlayerDefinition player)
    {
        _playerDefinitions[name] = player;
        return this;
    }

    /// <summary>Registers a player definition.</summary>
    public GameDefinitionBuilder AddPlayer(string name, IReadOnlyDictionary<string, object> staticProperties,
        IReadOnlyList<StateFieldDecl>? stateMapDeclarations = null)
        => AddPlayer(name, new PlayerDefinition(staticProperties, stateMapDeclarations));

    // -----------------------------------------------------------------------
    //  Session state map
    // -----------------------------------------------------------------------

    /// <summary>
    /// Declares additional state fields on the singleton session atom.
    /// The engine-reserved fields (<c>turn-number</c>, <c>phase-index</c>) are
    /// implicitly declared and must not appear here.
    /// </summary>
    public GameDefinitionBuilder WithSessionStateMap(IReadOnlyList<StateFieldDecl> declarations)
    {
        _sessionStateMap = declarations;
        return this;
    }

    // -----------------------------------------------------------------------
    //  InitManifest
    // -----------------------------------------------------------------------

    /// <summary>Sets the required <see cref="InitManifest"/>.</summary>
    public GameDefinitionBuilder WithInitManifest(InitManifest manifest)
    {
        _initManifest = manifest;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Playable zone names (D19)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Restricts <c>PlayCard</c> actions to cards in the named zones.
    /// <c>null</c> (default) means no zone restriction.
    /// </summary>
    public GameDefinitionBuilder WithPlayableZones(params string[] zoneNames)
    {
        _playableZoneNames = zoneNames;
        return this;
    }

    // -----------------------------------------------------------------------
    //  Atom groups
    // -----------------------------------------------------------------------

    /// <summary>
    /// Registers a build-time group that selects card definitions by
    /// <paramref name="matcher"/> and transforms them via <paramref name="transform"/>.
    /// Groups are applied in ascending <paramref name="priority"/> order during
    /// <see cref="Build"/>, after all cards have been registered.
    /// </summary>
    public GameDefinitionBuilder RegisterCardGroup(
        string name,
        Func<CardDefinition, bool> matcher,
        Func<CardDefinition, CardDefinition> transform,
        int priority = 0)
    {
        _cardGroups.Add(new AtomGroup<CardDefinition>(name, matcher, transform, priority));
        return this;
    }

    /// <summary>
    /// Registers a build-time group that selects zone definitions by
    /// <paramref name="matcher"/> and transforms them via <paramref name="transform"/>.
    /// </summary>
    public GameDefinitionBuilder RegisterZoneGroup(
        string name,
        Func<ZoneDefinition, bool> matcher,
        Func<ZoneDefinition, ZoneDefinition> transform,
        int priority = 0)
    {
        _zoneGroups.Add(new AtomGroup<ZoneDefinition>(name, matcher, transform, priority));
        return this;
    }

    /// <summary>
    /// Registers a build-time group that selects player definitions by
    /// <paramref name="matcher"/> and transforms them via <paramref name="transform"/>.
    /// </summary>
    public GameDefinitionBuilder RegisterPlayerGroup(
        string name,
        Func<PlayerDefinition, bool> matcher,
        Func<PlayerDefinition, PlayerDefinition> transform,
        int priority = 0)
    {
        _playerGroups.Add(new AtomGroup<PlayerDefinition>(name, matcher, transform, priority));
        return this;
    }

    // -----------------------------------------------------------------------
    //  Build
    // -----------------------------------------------------------------------

    /// <summary>
    /// Validates the registered data and returns an immutable
    /// <see cref="GameDefinition"/>.
    /// </summary>
    /// <exception cref="DefinitionException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><see cref="InitManifest"/> was not set.</item>
    ///   <item>A game-creator keyword name shadows a built-in keyword name.</item>
    ///   <item>A game-creator keyword body contains a <see cref="ParameterRef"/>
    ///         whose name is not in the keyword's declared parameters.</item>
    ///   <item>A game-creator keyword body invokes an unknown keyword name.</item>
    /// </list>
    /// </exception>
    public GameDefinition Build()
    {
        if (_initManifest is null)
            throw new DefinitionException("GameDefinitionBuilder.Build() requires WithInitManifest() to be called.");

        // Merge built-ins first.
        var builtInByName = BuiltInKeywords.All.ToDictionary(k => k.Name, k => k);

        // Check for shadowing.
        foreach (var name in _keywords.Keys)
        {
            if (builtInByName.ContainsKey(name))
                throw new DefinitionException(
                    $"Game-creator keyword '{name}' shadows a built-in keyword. Choose a different name.");
        }

        // Merge all keywords (built-ins + game-creator).
        var allKeywords = new Dictionary<string, KeywordDefinition>(builtInByName);
        foreach (var (name, kw) in _keywords)
            allKeywords[name] = kw;

        // Validate each game-creator keyword body.
        foreach (var kw in _keywords.Values)
        {
            if (kw.Body is not null)
            {
                var paramNames = kw.Parameters.Select(p => p.Name).ToHashSet();
                ValidateNode(kw.Body, paramNames, allKeywords.Keys.ToHashSet(), kw.Name);
            }
        }

        var cardDefs   = ApplyGroups(_cardDefinitions,   _cardGroups);
        var zoneDefs   = ApplyGroups(_zoneDefinitions,   _zoneGroups);
        var playerDefs = ApplyGroups(_playerDefinitions, _playerGroups);

        var definition = new GameDefinition(
            Keywords:                    allKeywords,
            CardDefinitions:             cardDefs,
            ZoneDefinitions:             zoneDefs,
            StateBasedRules:             _stateBasedRules,
            Phases:                      _phases,
            ActionRules:                 _actionRules,
            TriggerResolutionOrder:      _triggerResolutionOrder,
            PlayerDefinitions:           playerDefs,
            InitManifest:                _initManifest,
            PlayableZoneNames:           _playableZoneNames,
            Id:                          _id,
            SessionStateMapDeclarations: _sessionStateMap);

        // Validate state-map field references in keyword bodies and effect blocks.
        StateMapValidator.Validate(definition);

        return definition;
    }

    // -----------------------------------------------------------------------
    //  Group application
    // -----------------------------------------------------------------------

    private static Dictionary<string, TDef> ApplyGroups<TDef>(
        Dictionary<string, TDef> source,
        List<AtomGroup<TDef>> groups)
    {
        if (groups.Count == 0)
            return source;

        var result = new Dictionary<string, TDef>(source);
        foreach (var group in groups.OrderBy(g => g.Priority))
        {
            foreach (var key in result.Keys.ToList())
            {
                if (group.Matcher(result[key]))
                    result[key] = group.Transform(result[key]);
            }
        }
        return result;
    }

    // -----------------------------------------------------------------------
    //  Validation helpers
    // -----------------------------------------------------------------------

    private static void ValidateNode(
        KeywordNode node,
        HashSet<string> paramNames,
        HashSet<string> knownKeywords,
        string owningKeyword)
    {
        switch (node)
        {
            case ParameterRef pr:
                if (!paramNames.Contains(pr.Name))
                    throw new DefinitionException(
                        $"Keyword '{owningKeyword}' body references unknown parameter '{pr.Name}'. " +
                        $"Declared parameters: [{string.Join(", ", paramNames)}].");
                break;

            case Literal:
                break; // Always valid.

            case Invocation inv:
                if (!knownKeywords.Contains(inv.KeywordName))
                    throw new DefinitionException(
                        $"Keyword '{owningKeyword}' body invokes unknown keyword '{inv.KeywordName}'. " +
                        "Register it with RegisterKeyword(KeywordDefinition) before calling Build().");
                foreach (var arg in inv.Args)
                    ValidateNode(arg, paramNames, knownKeywords, owningKeyword);
                break;
        }
    }
}

/// <summary>
/// A named, build-time transformation applied to all atom definitions
/// that satisfy <see cref="Matcher"/>, in ascending <see cref="Priority"/> order.
/// </summary>
public sealed record AtomGroup<TDef>(
    string Name,
    Func<TDef, bool> Matcher,
    Func<TDef, TDef> Transform,
    int Priority);
