using System.Text.Json;
using Archetype.Core;

namespace Archetype.Tooling.Server.Export;

// ---------------------------------------------------------------------------
//  GameDefinitionExporter — builds a strict GameDefinition from ProjectState
//  and serialises it to the engine's JSON format (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Converts a validated <see cref="ProjectState"/> into a strict
/// <see cref="GameDefinition"/> and serialises it to JSON.
/// <para>
/// <b>Export is blocked when <see cref="ProjectState.ErrorCount"/> &gt; 0.</b>
/// Missing-translation warnings are surfaced separately; the caller controls
/// the <c>force</c> override (D31).
/// </para>
/// </summary>
public static class GameDefinitionExporter
{
    // D27: GameDefinition export requires LiteralConverter to be registered for
    // correct KeywordNode polymorphic serialisation (see GameDefinitionJsonOptions).
    private static readonly JsonSerializerOptions _opts =
        GameDefinitionJsonOptions.Build(writeIndented: true);

    /// <summary>
    /// Attempts to export the project as a <c>GameDefinition</c> JSON string.
    /// Returns <see cref="ExportResult"/> describing success, errors, or the
    /// missing-translation gate (D31).
    /// </summary>
    public static ExportResult Export(ProjectState state, bool force = false)
    {
        // Step 1: hard-error gate.
        if (state.ErrorCount > 0)
            return ExportResult.Blocked(
                $"{state.ErrorCount} error(s) must be fixed before exporting.");

        // Step 2: missing-translation gate (D31).
        var missingTranslations = BuildMissingTranslationSummary(state);
        if (missingTranslations.Count > 0 && !force)
            return ExportResult.WithMissingTranslations(missingTranslations);

        // Step 3: build strict GameDefinition.
        GameDefinition def;
        try
        {
            def = BuildGameDefinition(state);
        }
        catch (Exception ex)
        {
            return ExportResult.Blocked($"Export failed: {ex.Message}");
        }

        // Step 4: serialise to JSON.
        var json = JsonSerializer.Serialize(def, _opts);
        return ExportResult.Success(json);
    }

    // -----------------------------------------------------------------------
    //  Build GameDefinition from ProjectState
    // -----------------------------------------------------------------------

    private static GameDefinition BuildGameDefinition(ProjectState state)
    {
        // Keywords: built-ins are merged automatically by GameDefinition.
        var keywords = new Dictionary<string, KeywordDefinition>(StringComparer.Ordinal);
        foreach (var (name, kw) in state.Keywords)
        {
            if (kw.BodyNode is null)
                throw new InvalidOperationException(
                    $"Keyword '{name}' has parse errors; cannot export.");

            // D27: use the declared ReturnType; hardcoding Atom is incorrect.
            keywords[name] = new KeywordDefinition(
                Name:         name,
                Parameters:   kw.Parameters.ToArray(),
                ReturnType:   kw.ReturnType ?? TypeName.Atom,
                Description:  "",
                Body:         kw.BodyNode,
                TextTemplate: kw.TextTemplate);
        }

        // Merge built-ins.
        foreach (var b in BuiltInKeywords.All)
            keywords[b.Name] = b;

        // Cards
        var cards = new Dictionary<string, CardDefinition>(StringComparer.Ordinal);
        foreach (var (name, card) in state.Cards)
        {
            if (card.PrimaryEffectNode is null)
                throw new InvalidOperationException(
                    $"Card '{name}' primary effect has parse errors; cannot export.");

            var additionalEffects = card.AdditionalEffects
                .Select(e => new NamedEffectBlockDef(
                    Name:               e.Name,
                    ActivationCondition: e.ActivationConditionNode,
                    Cost:               e.Costs
                        .Where(c => c.BodyNode is not null)
                        .Select(c => new CostDef(c.BodyNode!, [], c.TextTemplate))
                        .ToList(),
                    Body:               e.BodyNode ?? EffectBlockDef.Empty))
                .ToList();

            var costs = card.Costs
                .Where(c => c.BodyNode is not null)
                .Select(c => new CostDef(c.BodyNode!, [], c.TextTemplate))
                .ToList();

            var staticEffects = BuildStaticEffectDefs(card.StaticEffects);

            cards[name] = new CardDefinition(
                Name:               name,
                StaticProperties:   card.StaticProperties,
                PrimaryEffect:      card.PrimaryEffectNode,
                AdditionalEffects:  additionalEffects,
                StaticEffects:      staticEffects,
                ActivationCondition: card.ActivationConditionNode,
                Cost:               costs.Count > 0 ? costs : null);
        }

        // Zones
        var zones = new Dictionary<string, ZoneDefinition>(StringComparer.Ordinal);
        foreach (var (name, zone) in state.Zones)
            zones[name] = new ZoneDefinition(name, zone.StaticProperties);

        // Card sets
        var cardSets = new Dictionary<string, CardSet>(StringComparer.Ordinal);
        foreach (var (name, cs) in state.CardSets)
            cardSets[name] = new CardSet(name, cs.Cards);

        // State-based rules
        var sbrs = state.StateBasedRules
            .Where(r => r.ConditionNode is not null && r.BodyNode is not null)
            .Select(r => new StateBasedRule(r.Name, r.ConditionNode!, r.BodyNode!))
            .ToList();

        // Phases
        var phases = state.Phases
            .Select(ph => new PhaseDefinition(ph.Name, ph.InitNode, ph.CleanupNode))
            .ToList();

        // Action rules
        var actionRules = new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(
            StringComparer.Ordinal);
        foreach (var (actionType, rules) in state.ActionRules)
        {
            actionRules[actionType] = rules
                .Select(r => new ActionRuleDefinition(r.BeforeNode, r.AfterNode))
                .ToList();
        }

        // Player definitions
        var players = new Dictionary<string, PlayerDefinition>(StringComparer.Ordinal);
        foreach (var (name, player) in state.Players)
            players[name] = new PlayerDefinition(player.StaticProperties);

        // InitManifest
        InitManifest initManifest;
        if (state.InitManifest is { } im)
        {
            initManifest = new InitManifest(im.Zones, im.Cards, im.Players);
        }
        else
        {
            initManifest = InitManifest.Empty;
        }

        return new GameDefinition(
            Keywords:              keywords,
            CardDefinitions:       cards,
            ZoneDefinitions:       zones,
            CardSets:              cardSets,
            StateBasedRules:       sbrs,
            Phases:                phases,
            ActionRules:           actionRules,
            TriggerResolutionOrder: state.TriggerResolutionOrder,
            PlayerDefinitions:     players,
            InitManifest:          initManifest,
            PlayableZoneNames:     state.PlayableZoneNames.Count > 0
                                        ? state.PlayableZoneNames : null,
            Id:                    state.Id);
    }

    // -----------------------------------------------------------------------
    //  Static effect mapping
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps <see cref="StaticEffectEntry"/> values to <see cref="StaticEffectDef"/> values.
    /// Entries whose <see cref="StaticEffectEntry.ContributionNode"/> is null are skipped
    /// (parse error — validator should have already reported this).
    /// Trigger is omitted when either <see cref="StaticEffectEntry.TriggerConditionNode"/>
    /// or <see cref="StaticEffectEntry.TriggerBodyNode"/> is null (D27).
    /// </summary>
    private static List<StaticEffectDef> BuildStaticEffectDefs(
        IReadOnlyList<StaticEffectEntry> entries)
    {
        var result = new List<StaticEffectDef>();
        foreach (var e in entries)
        {
            // ContributionNode is required; skip broken entries.
            if (e.ContributionNode is null) continue;

            // Build trigger only when both condition node and body node are present.
            TriggerDefinition? trigger = null;
            if (e.TriggerConditionNode is not null &&
                e.TriggerBodyNode is not null &&
                e.TriggerEventKeyword is { Length: > 0 })
            {
                trigger = new TriggerDefinition(
                    EventKeyword:  e.TriggerEventKeyword,
                    Scope:         e.TriggerScope,
                    EventParams:   [],
                    Condition:     e.TriggerConditionNode,
                    EventBindings: [],
                    FiredBlock:    e.TriggerBodyNode);
            }

            result.Add(new StaticEffectDef(
                Lifetime:               e.LifetimeNode ?? LifetimeSpec.Permanent,
                StateContributionBlock: e.ContributionNode,
                Trigger:                trigger));
        }
        return result;
    }

    // -----------------------------------------------------------------------
    //  Missing-translation summary (D31)
    // -----------------------------------------------------------------------

    private static List<MissingTranslationEntry> BuildMissingTranslationSummary(
        ProjectState state)
    {
        var result = new List<MissingTranslationEntry>();
        var allLocales = state.Localization.Strings.Keys
            .Where(l => !string.Equals(l, state.Localization.SourceLanguage,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var locale in allLocales)
        {
            int count = state.Localization.MissingKeysForLocale(locale).Count;
            if (count > 0)
                result.Add(new MissingTranslationEntry(locale, count));
        }
        return result;
    }
}

// ---------------------------------------------------------------------------
//  Export result types
// ---------------------------------------------------------------------------

/// <summary>
/// The result of a <see cref="GameDefinitionExporter.Export"/> call.
/// Exactly one of <see cref="IsSuccess"/>, <see cref="IsBlocked"/>,
/// or <see cref="HasMissingTranslations"/> is true.
/// </summary>
public sealed record ExportResult
{
    /// <summary>Serialised <see cref="GameDefinition"/> JSON; non-null when <see cref="IsSuccess"/>.</summary>
    public string? Json { get; init; }
    /// <summary>Error message; non-null when <see cref="IsBlocked"/>.</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>Per-locale missing counts; non-empty when <see cref="HasMissingTranslations"/>.</summary>
    public IReadOnlyList<MissingTranslationEntry> MissingTranslations { get; init; } = [];

    /// <summary>True when export succeeded and <see cref="Json"/> is set.</summary>
    public bool IsSuccess             => Json is not null;
    /// <summary>True when hard errors blocked export.</summary>
    public bool IsBlocked             => ErrorMessage is not null;
    /// <summary>True when the missing-translation gate was reached (no force).</summary>
    public bool HasMissingTranslations => MissingTranslations.Count > 0 && !IsSuccess;

    /// <summary>Constructs a success result.</summary>
    public static ExportResult Success(string json) => new() { Json = json };
    /// <summary>Constructs a hard-error block result.</summary>
    public static ExportResult Blocked(string msg)  => new() { ErrorMessage = msg };
    /// <summary>Constructs a missing-translation gate result.</summary>
    public static ExportResult WithMissingTranslations(
        IReadOnlyList<MissingTranslationEntry> entries) =>
        new() { MissingTranslations = entries };
}

/// <summary>Per-locale missing translation count for the D31 export gate dialog.</summary>
public sealed record MissingTranslationEntry(string Locale, int MissingCount);
