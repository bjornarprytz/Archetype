using System.Text.Json;
using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  ProjectFileLoader — lenient .archetype project file reader (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Reads a <c>.archetype</c> JSON string into a <see cref="ProjectState"/>.
/// <para>
/// Loading is lenient: invalid JSON is reported as a fatal diagnostic and an
/// empty state is returned.  Individual DSL parse failures are recorded as
/// per-entry diagnostics; loading continues.  After all entries are loaded,
/// <see cref="ReferenceGraph.Build"/> and the cross-entry name-resolution
/// pass run to populate <see cref="ProjectState.Diagnostics"/>.
/// </para>
/// </summary>
public static class ProjectFileLoader
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Parses <paramref name="json"/> and returns a populated
    /// <see cref="ProjectState"/>.  Never throws — all errors become
    /// <see cref="ProjectDiagnostic"/> entries.
    /// </summary>
    public static ProjectState Load(string json)
    {
        var state = new ProjectState();

        // Step 1: parse top-level JSON.
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling     = JsonCommentHandling.Skip,
            });
        }
        catch (JsonException ex)
        {
            state.Diagnostics.Add(new ProjectDiagnostic(
                "project", "", "error",
                $"Invalid JSON: {ex.Message}"));
            return state;
        }

        var root = doc.RootElement;

        // Identity
        state.Id = root.TryGetString("id");

        // Trigger resolution order
        if (root.TryGetProperty("triggerResolutionOrder", out var tro) &&
            Enum.TryParse<TriggerResolutionOrder>(tro.GetString(), out var order))
            state.TriggerResolutionOrder = order;

        // Playable zone names
        if (root.TryGetProperty("playableZoneNames", out var pzn))
        {
            foreach (var el in pzn.EnumerateArray())
            {
                var s = el.GetString();
                if (s is not null) state.PlayableZoneNames.Add(s);
            }
        }

        // Step 2: load each keyword entry; parse DSL bodies.
        if (root.TryGetProperty("keywords", out var kws))
        {
            foreach (var kw in kws.EnumerateObject())
            {
                var entry = LoadKeywordEntry(kw.Name, kw.Value, state);
                state.Keywords[entry.Name] = entry;
            }
        }

        // Cards
        if (root.TryGetProperty("cards", out var cards))
        {
            foreach (var c in cards.EnumerateObject())
            {
                var entry = LoadCardEntry(c.Name, c.Value, state);
                state.Cards[entry.Name] = entry;
            }
        }

        // Zones
        if (root.TryGetProperty("zones", out var zones))
        {
            foreach (var z in zones.EnumerateObject())
            {
                state.Zones[z.Name] = new ZoneEntry
                {
                    Name             = z.Name,
                    StaticProperties = LoadStaticProperties(z.Value),
                };
            }
        }

        // Players
        if (root.TryGetProperty("players", out var players))
        {
            foreach (var p in players.EnumerateObject())
            {
                state.Players[p.Name] = new PlayerEntry
                {
                    Name             = p.Name,
                    StaticProperties = LoadStaticProperties(p.Value),
                };
            }
        }

        // Card sets
        if (root.TryGetProperty("cardSets", out var cardSets))
        {
            foreach (var cs in cardSets.EnumerateObject())
            {
                var entry = new CardSetEntry { Name = cs.Name };
                if (cs.Value.TryGetProperty("cards", out var csCards))
                    foreach (var card in csCards.EnumerateArray())
                    {
                        var cn = card.GetString();
                        if (cn is not null) entry.Cards.Add(cn);
                    }
                state.CardSets[cs.Name] = entry;
            }
        }

        // Phases
        if (root.TryGetProperty("phases", out var phases))
        {
            foreach (var ph in phases.EnumerateArray())
            {
                var entry = new PhaseEntry
                {
                    Name       = ph.TryGetString("name") ?? "",
                    InitDsl    = ph.TryGetString("init"),
                    CleanupDsl = ph.TryGetString("cleanup"),
                };
                if (entry.InitDsl is { Length: > 0 } idsl)
                {
                    var r = DslParser.ParseBlock(idsl);
                    entry.InitNode = r.IsSuccess ? r.Block : null;
                    AddBlockDiagnostics(r, "phase", entry.Name, "init", entry.Diagnostics);
                }
                if (entry.CleanupDsl is { Length: > 0 } cdsl)
                {
                    var r = DslParser.ParseBlock(cdsl);
                    entry.CleanupNode = r.IsSuccess ? r.Block : null;
                    AddBlockDiagnostics(r, "phase", entry.Name, "cleanup", entry.Diagnostics);
                }
                state.Phases.Add(entry);
            }
        }

        // Action rules
        if (root.TryGetProperty("actionRules", out var actionRules))
        {
            foreach (var ar in actionRules.EnumerateObject())
            {
                var list = new List<ActionRuleEntry>();
                foreach (var ruleEl in ar.Value.EnumerateArray())
                {
                    var ruleEntry = new ActionRuleEntry
                    {
                        BeforeDsl = ruleEl.TryGetString("before"),
                        AfterDsl  = ruleEl.TryGetString("after"),
                    };
                    if (ruleEntry.BeforeDsl is { Length: > 0 } bdsl)
                    {
                        var r = DslParser.ParseBlock(bdsl);
                        ruleEntry.BeforeNode = r.IsSuccess ? r.Block : null;
                        AddBlockDiagnostics(r, "actionRule", ar.Name, "before", ruleEntry.Diagnostics);
                    }
                    if (ruleEntry.AfterDsl is { Length: > 0 } adsl)
                    {
                        var r = DslParser.ParseBlock(adsl);
                        ruleEntry.AfterNode = r.IsSuccess ? r.Block : null;
                        AddBlockDiagnostics(r, "actionRule", ar.Name, "after", ruleEntry.Diagnostics);
                    }
                    list.Add(ruleEntry);
                }
                state.ActionRules[ar.Name] = list;
            }
        }

        // State-based rules
        if (root.TryGetProperty("stateBasedRules", out var sbrs))
        {
            foreach (var sbr in sbrs.EnumerateArray())
            {
                var entry = new StateBasedRuleEntry
                {
                    Name         = sbr.TryGetString("name") ?? "",
                    ConditionDsl = sbr.TryGetString("condition") ?? "",
                    BodyDsl      = sbr.TryGetString("body") ?? "",
                };
                var condResult = DslParser.Parse(entry.ConditionDsl);
                if (condResult.IsSuccess) entry.ConditionNode = condResult.Node;
                else entry.Diagnostics.Add(ParseError("stateBasedRule", entry.Name,
                    condResult.ErrorMessage!, condResult.ErrorOffset, null));

                var bodyResult = DslParser.ParseBlock(entry.BodyDsl);
                entry.BodyNode = bodyResult.IsSuccess ? bodyResult.Block : null;
                AddBlockDiagnostics(bodyResult, "stateBasedRule", entry.Name, "body", entry.Diagnostics);

                state.StateBasedRules.Add(entry);
            }
        }

        // InitManifest
        if (root.TryGetProperty("initManifest", out var im))
        {
            state.InitManifest = LoadInitManifestEntry(im, state);
        }

        // Localization
        if (root.TryGetProperty("localization", out var loc))
        {
            if (loc.TryGetString("sourceLanguage") is { } sl)
                state.Localization.SourceLanguage = sl;
            if (loc.TryGetProperty("strings", out var strings))
            {
                foreach (var locale in strings.EnumerateObject())
                {
                    var table = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var kv in locale.Value.EnumerateObject())
                    {
                        var s = kv.Value.GetString();
                        if (s is not null) table[kv.Name] = s;
                    }
                    state.Localization.Strings[locale.Name] = table;
                }
            }
        }

        // Tooling editor state (round-trip verbatim)
        if (root.TryGetProperty("tooling", out var tooling) &&
            tooling.TryGetProperty("editorState", out var editorState))
            state.EditorState = editorState.Clone();

        // Step 3: cross-entry validation
        ReferenceGraph.Build(state);
        Validator.Validate(state);

        return state;
    }

    // -----------------------------------------------------------------------
    //  Per-entry loaders
    // -----------------------------------------------------------------------

    private static KeywordEntry LoadKeywordEntry(
        string name, JsonElement el, ProjectState state)
    {
        var entry = new KeywordEntry
        {
            Name         = name,
            Parameters   = LoadParameterDecls(el),
            BodyDsl      = el.TryGetString("body") ?? "",
            TextTemplate = el.TryGetString("textTemplate"),
        };

        // Parse signal behaviour from annotations
        if (el.TryGetString("signalBehaviour") is { } sb &&
            Enum.TryParse<SignalBehaviour>(sb, out var behaviour))
            entry.SignalBehaviour = behaviour;

        // Parse DSL body
        if (entry.BodyDsl.Length > 0)
        {
            var result = DslParser.Parse(entry.BodyDsl);
            if (result.IsSuccess)
                entry.BodyNode = result.Node;
            else
                entry.Diagnostics.Add(ParseError(
                    "keyword", name, result.ErrorMessage!,
                    result.ErrorOffset, result.ErrorOffset.HasValue
                        ? new DslRange(result.ErrorOffset.Value, entry.BodyDsl.Length)
                        : null));
        }

        return entry;
    }

    private static CardEntry LoadCardEntry(
        string name, JsonElement el, ProjectState state)
    {
        var entry = new CardEntry
        {
            Name             = name,
            StaticProperties = LoadStaticProperties(el),
            PrimaryEffectDsl = el.TryGetString("primaryEffect") ?? "",
            FlavourText      = el.TryGetString("flavourText"),
            ArtPath          = el.TryGetString("artPath"),
        };

        // Parse activation condition
        if (el.TryGetString("activationCondition") is { Length: > 0 } acdsl)
        {
            entry.ActivationConditionDsl = acdsl;
            var r = DslParser.Parse(acdsl);
            if (r.IsSuccess) entry.ActivationConditionNode = r.Node;
            else entry.Diagnostics.Add(ParseError(
                "card", name, r.ErrorMessage!, r.ErrorOffset, null));
        }

        // Parse primary effect
        if (entry.PrimaryEffectDsl.Length > 0)
        {
            var r = DslParser.ParseBlock(entry.PrimaryEffectDsl);
            entry.PrimaryEffectNode = r.IsSuccess ? r.Block : null;
            AddBlockDiagnostics(r, "card", name, "primaryEffect", entry.Diagnostics);
        }

        // Additional effects
        if (el.TryGetProperty("additionalEffects", out var addl))
        {
            foreach (var ae in addl.EnumerateObject())
            {
                var eff = new NamedEffectEntry { Name = ae.Name };
                eff.BodyDsl = ae.Value.TryGetString("body") ?? "";
                if (eff.BodyDsl.Length > 0)
                {
                    var r = DslParser.ParseBlock(eff.BodyDsl);
                    eff.BodyNode = r.IsSuccess ? r.Block : null;
                    AddBlockDiagnostics(r, "card", name, $"effect:{ae.Name}", entry.Diagnostics);
                }
                // Costs for the ability
                eff.Costs = LoadCostEntries(ae.Value, "card", name, entry.Diagnostics);
                entry.AdditionalEffects.Add(eff);
            }
        }

        // Card-level costs
        entry.Costs = LoadCostEntries(el, "card", name, entry.Diagnostics);

        return entry;
    }

    private static List<CostEntry> LoadCostEntries(
        JsonElement el, string entryKind, string entryName,
        List<ProjectDiagnostic> diagnostics)
    {
        var list = new List<CostEntry>();
        if (!el.TryGetProperty("costs", out var costs)) return list;
        foreach (var c in costs.EnumerateArray())
        {
            var cost = new CostEntry
            {
                BodyDsl      = c.TryGetString("body") ?? "",
                TextTemplate = c.TryGetString("textTemplate"),
            };
            if (cost.BodyDsl.Length > 0)
            {
                var r = DslParser.ParseBlock(cost.BodyDsl);
                cost.BodyNode = r.IsSuccess ? r.Block : null;
                AddBlockDiagnostics(r, entryKind, entryName, "cost", diagnostics);
            }
            list.Add(cost);
        }
        return list;
    }

    private static InitManifestEntry LoadInitManifestEntry(JsonElement el, ProjectState state)
    {
        var entry = new InitManifestEntry();

        if (el.TryGetProperty("zones", out var zones))
        {
            foreach (var z in zones.EnumerateArray())
            {
                var owner = z.TryGetString("owner") ?? "";
                var localId = z.TryGetString("localId");
                // ZoneSpec needs the definition name and the owner player name
                var defName = z.TryGetString("definition") ?? z.TryGetString("name") ?? "";
                entry.Zones.Add(new ZoneSpec(owner, defName, localId ?? defName));
            }
        }

        if (el.TryGetProperty("cards", out var cards))
        {
            foreach (var c in cards.EnumerateArray())
            {
                entry.Cards.Add(new CardSpec(
                    Owner:      c.TryGetString("owner") ?? "",
                    ZoneLocalId: c.TryGetString("zoneLocalId") ?? "",
                    Definition: c.TryGetString("definition") ?? "",
                    LocalId:    c.TryGetString("localId")));
            }
        }

        if (el.TryGetProperty("players", out var players))
        {
            foreach (var p in players.EnumerateArray())
            {
                entry.Players.Add(new PlayerStateSpec(
                    Player: p.TryGetString("player") ?? ""));
            }
        }

        return entry;
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static List<ParameterDecl> LoadParameterDecls(JsonElement el)
    {
        var list = new List<ParameterDecl>();
        if (!el.TryGetProperty("parameters", out var ps)) return list;
        foreach (var p in ps.EnumerateArray())
        {
            var pName = p.TryGetString("name") ?? "";
            var typeName = TypeName.Atom;
            if (p.TryGetString("type") is { } typeStr &&
                Enum.TryParse<TypeName>(typeStr, ignoreCase: true, out var t))
                typeName = t;
            list.Add(new ParameterDecl(pName, typeName));
        }
        return list;
    }

    private static Dictionary<string, object> LoadStaticProperties(JsonElement el)
    {
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);
        if (!el.TryGetProperty("staticProperties", out var sp)) return dict;
        foreach (var kv in sp.EnumerateObject())
        {
            object val = kv.Value.ValueKind switch
            {
                JsonValueKind.Number  => kv.Value.GetDouble(),
                JsonValueKind.True    => true,
                JsonValueKind.False   => false,
                JsonValueKind.String  => kv.Value.GetString() ?? "",
                _                     => kv.Value.ToString(),
            };
            dict[kv.Name] = val;
        }
        return dict;
    }

    private static ProjectDiagnostic ParseError(
        string entryKind, string entryName, string message,
        int? offset, DslRange? range) =>
        new(entryKind, entryName, "error",
            message, range ?? (offset.HasValue ? new DslRange(offset.Value, offset.Value) : null));

    private static void AddBlockDiagnostics(
        BlockParseResult result, string entryKind, string entryName,
        string fieldName, List<ProjectDiagnostic> diagnostics)
    {
        foreach (var d in result.Diagnostics)
            diagnostics.Add(new ProjectDiagnostic(
                entryKind, entryName, "error",
                $"[{fieldName}] {d.Message}",
                new DslRange(d.Start, d.End)));
    }
}

// ---------------------------------------------------------------------------
//  JsonElement extension helpers
// ---------------------------------------------------------------------------

internal static class JsonElementExtensions
{
    internal static string? TryGetString(this JsonElement el, string property)
    {
        if (el.TryGetProperty(property, out var val) &&
            val.ValueKind == JsonValueKind.String)
            return val.GetString();
        return null;
    }
}
