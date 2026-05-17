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
    private readonly List<AtomGroup> _atomGroups = new();

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

    /// <summary>Registers an AtomGroup for build-time transformations.</summary>
    public GameDefinitionBuilder RegisterAtomGroup(AtomGroup group)
    {
        _atomGroups.Add(group);
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

        var cardDefinitions = new Dictionary<string, CardDefinition>(_cardDefinitions);

        if (_atomGroups.Count > 0)
        {
            foreach (var group in _atomGroups.OrderBy(g => g.Priority))
            {
                if (!group.Kinds.Contains(AtomKind.Card)) continue;
                foreach (var name in cardDefinitions.Keys.ToList())
                {
                    var card = cardDefinitions[name];
                    if (group.MatchesCard(name, card))
                    {
                        var transformed = group.TransformCard(card);
                        var merged = MergeCardDefinitions(card, transformed, group.OverrideLocal);
                        cardDefinitions[name] = merged;
                    }
                }
            }
        }

        var definition = new GameDefinition(
            Keywords:                    allKeywords,
            CardDefinitions:             cardDefinitions,
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

    private static CardDefinition MergeCardDefinitions(CardDefinition original, CardDefinition transformed, bool overrideLocal)
    {
        // Merge static properties: do not overwrite local keys unless overrideLocal is true.
        var mergedStatic = new Dictionary<string, object>(original.StaticProperties);
        foreach (var kv in transformed.StaticProperties)
        {
            if (mergedStatic.ContainsKey(kv.Key) && !overrideLocal)
                continue;
            mergedStatic[kv.Key] = kv.Value;
        }

        // Merge additional effects: preserve originals, append new ones by name.
        var additional = new List<NamedEffectBlockDef>(original.AdditionalEffects);
        var existingNames = new HashSet<string>(additional.Select(a => a.Name));
        foreach (var extra in transformed.AdditionalEffects)
            if (!existingNames.Contains(extra.Name))
                additional.Add(extra);

        // Merge static effects: append transformed effects (do not remove existing ones).
        var staticEffects = new List<StaticEffectDef>(original.StaticEffects);
        staticEffects.AddRange(transformed.StaticEffects);

        // ActivationCondition, Cost, StateMapDeclarations: only set if original did not declare them or overrideLocal==true
        var activation = (!overrideLocal && original.ActivationCondition is not null) ? original.ActivationCondition : transformed.ActivationCondition;
        var cost = (!overrideLocal && original.Cost is not null) ? original.Cost : transformed.Cost;
        var stateMap = (!overrideLocal && original.StateMapDeclarations is not null) ? original.StateMapDeclarations : transformed.StateMapDeclarations;

        // PrimaryEffect: prefer original unless overrideLocal requested and transformed differs.
        var primary = (!overrideLocal && !ReferenceEquals(original.PrimaryEffect, transformed.PrimaryEffect)) ? original.PrimaryEffect : transformed.PrimaryEffect;

        return new CardDefinition(original.Name, mergedStatic, primary, additional, staticEffects, activation, cost, stateMap);
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
