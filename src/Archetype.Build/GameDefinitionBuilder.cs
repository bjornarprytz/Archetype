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
///     .RegisterKeyword("take-damage",
///         parameters: [new ParameterDecl("target", TypeName.Card), new ParameterDecl("amount", TypeName.Number)],
///         body: Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Num(-1)),
///         textTemplate: "{target} takes {amount} damage")
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
    public GameDefinitionBuilder AddZone(string name, IReadOnlyDictionary<string, object> staticProperties)
        => AddZone(name, staticProperties, stateMapDeclarations: null);

    /// <summary>
    /// Registers a zone definition with explicit state map declarations.
    /// Existing callers that omit <paramref name="stateMapDeclarations"/> continue
    /// to compile unchanged.
    /// </summary>
    public GameDefinitionBuilder AddZone(
        string name,
        IReadOnlyDictionary<string, object> staticProperties,
        IReadOnlyList<StateFieldDecl>? stateMapDeclarations)
    {
        _zoneDefinitions[name] = new ZoneDefinition(name, staticProperties, stateMapDeclarations);
        return this;
    }

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
    /// Registers a named game-creator keyword.
    /// <para>
    /// The <paramref name="body"/> should be constructed by calling game-creator
    /// or <see cref="Kw"/> methods with <see cref="Kw.Param"/> placeholders for
    /// each declared parameter. <c>Build()</c> validates that all
    /// <see cref="ParameterRef"/> names in the body appear in <paramref name="parameters"/>.
    /// </para>
    /// </summary>
    public GameDefinitionBuilder RegisterKeyword(
        string name,
        ParameterDecl[] parameters,
        KeywordNode body,
        string? textTemplate = null,
        string? description = null,
        TypeName returnType = TypeName.Number)
    {
        _keywords[name] = new KeywordDefinition(
            Name: name,
            Parameters: parameters,
            ReturnType: returnType,
            Description: description ?? name,
            Body: body,
            PrimitiveSentinel: null,
            TextTemplate: textTemplate);
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
    public GameDefinitionBuilder AddPlayer(string name, IReadOnlyDictionary<string, object> staticProperties)
        => AddPlayer(name, staticProperties, stateMapDeclarations: null);

    /// <summary>
    /// Registers a player definition with explicit state map declarations.
    /// Existing callers that omit <paramref name="stateMapDeclarations"/> continue
    /// to compile unchanged.
    /// </summary>
    public GameDefinitionBuilder AddPlayer(
        string name,
        IReadOnlyDictionary<string, object> staticProperties,
        IReadOnlyList<StateFieldDecl>? stateMapDeclarations)
    {
        _playerDefinitions[name] = new PlayerDefinition(staticProperties, stateMapDeclarations);
        return this;
    }

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

        var definition = new GameDefinition(
            Keywords:                    allKeywords,
            CardDefinitions:             _cardDefinitions,
            ZoneDefinitions:             _zoneDefinitions,
            StateBasedRules:             _stateBasedRules,
            Phases:                      _phases,
            ActionRules:                 _actionRules,
            TriggerResolutionOrder:      _triggerResolutionOrder,
            PlayerDefinitions:           _playerDefinitions,
            InitManifest:                _initManifest,
            PlayableZoneNames:           _playableZoneNames,
            Id:                          _id,
            SessionStateMapDeclarations: _sessionStateMap);

        // Validate state-map field references in keyword bodies and effect blocks.
        StateMapValidator.Validate(definition);

        return definition;
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
                        "Register it with RegisterKeyword() before calling Build().");
                foreach (var arg in inv.Args)
                    ValidateNode(arg, paramNames, knownKeywords, owningKeyword);
                break;
        }
    }
}
