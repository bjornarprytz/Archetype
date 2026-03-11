using System.Text.Json;
using System.Text.Json.Nodes;
using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  ProjectFileSerializer — writes ProjectState to .archetype JSON (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Serialises a <see cref="ProjectState"/> to the <c>.archetype</c> JSON format.
/// <para>
/// DSL source strings are written (not <see cref="KeywordNode"/> trees).
/// The <c>tooling.editorState</c> section is round-tripped verbatim from
/// <see cref="ProjectState.EditorState"/>.
/// </para>
/// </summary>
public static class ProjectFileSerializer
{
    private static readonly JsonSerializerOptions _writeOpts = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Serialises <paramref name="state"/> to a UTF-8 JSON string.
    /// The result is suitable for writing directly to a <c>.archetype</c> file.
    /// </summary>
    public static string Serialize(ProjectState state)
    {
        var root = new JsonObject();

        root["version"] = 1;
        if (state.Id is not null)
            root["id"] = state.Id;

        root["triggerResolutionOrder"] = state.TriggerResolutionOrder.ToString();

        // Keywords
        var kws = new JsonObject();
        foreach (var (name, kw) in state.Keywords)
        {
            var kwObj = new JsonObject();
            // D27: returnType is mandatory; omit only when null (e.g. draft entries).
            if (kw.ReturnType is { } rt)
                kwObj["returnType"] = rt.ToString();
            kwObj["body"] = kw.BodyDsl;
            if (kw.TextTemplate is not null)
                kwObj["textTemplate"] = kw.TextTemplate;
            if (kw.SignalBehaviour != SignalBehaviour.Default)
                kwObj["signalBehaviour"] = kw.SignalBehaviour.ToString();

            var ps = new JsonArray();
            foreach (var p in kw.Parameters)
            {
                var pd = new JsonObject();
                pd["name"] = p.Name;
                pd["type"] = p.Type.ToString();
                ps.Add(pd);
            }
            kwObj["parameters"] = ps;
            kws[name] = kwObj;
        }
        root["keywords"] = kws;

        // Cards
        var cards = new JsonObject();
        foreach (var (name, card) in state.Cards)
        {
            var cardObj = new JsonObject();
            cardObj["primaryEffect"] = card.PrimaryEffectDsl;
            if (card.ActivationConditionDsl is not null)
                cardObj["activationCondition"] = card.ActivationConditionDsl;
            if (card.FlavourText is not null)
                cardObj["flavourText"] = card.FlavourText;
            if (card.ArtPath is not null)
                cardObj["artPath"] = card.ArtPath;
            // D27: artCropRegion round-trip — omitting this causes silent data loss on save.
            if (card.ArtCropRegion is { } crop && crop.Length == 4)
            {
                var arr = new JsonArray();
                foreach (var f in crop) arr.Add(f);
                cardObj["artCropRegion"] = arr;
            }

            // Static properties
            if (card.StaticProperties.Count > 0)
            {
                var sp = new JsonObject();
                foreach (var (k, v) in card.StaticProperties)
                    sp[k] = v is double d ? JsonValue.Create(d)
                           : v is bool b   ? JsonValue.Create(b)
                                           : JsonValue.Create(v.ToString());
                cardObj["staticProperties"] = sp;
            }

            // Costs
            if (card.Costs.Count > 0)
                cardObj["costs"] = SerializeCostEntries(card.Costs);

            // Additional effects
            if (card.AdditionalEffects.Count > 0)
            {
                var ae = new JsonObject();
                foreach (var eff in card.AdditionalEffects)
                {
                    var effObj = new JsonObject();
                    effObj["body"] = eff.BodyDsl;
                    if (eff.ActivationConditionDsl is not null)
                        effObj["activationCondition"] = eff.ActivationConditionDsl;
                    if (eff.Costs.Count > 0)
                        effObj["costs"] = SerializeCostEntries(eff.Costs);
                    ae[eff.Name] = effObj;
                }
                cardObj["additionalEffects"] = ae;
            }

            // Static effects (D27: not a stub — serialize all fields including lifetime).
            if (card.StaticEffects.Count > 0)
                cardObj["staticEffects"] = SerializeStaticEffectEntries(card.StaticEffects);

            cards[name] = cardObj;
        }
        root["cards"] = cards;

        // Zones
        var zones = new JsonObject();
        foreach (var (name, zone) in state.Zones)
        {
            var zoneObj = new JsonObject();
            if (zone.StaticProperties.Count > 0)
            {
                var sp = new JsonObject();
                foreach (var (k, v) in zone.StaticProperties)
                    sp[k] = v?.ToString();
                zoneObj["staticProperties"] = sp;
            }
            zones[name] = zoneObj;
        }
        root["zones"] = zones;

        // Players
        var players = new JsonObject();
        foreach (var (name, player) in state.Players)
        {
            var pObj = new JsonObject();
            if (player.StaticProperties.Count > 0)
            {
                var sp = new JsonObject();
                foreach (var (k, v) in player.StaticProperties)
                    sp[k] = v?.ToString();
                pObj["staticProperties"] = sp;
            }
            players[name] = pObj;
        }
        root["players"] = players;

        // Card sets
        var cardSets = new JsonObject();
        foreach (var (name, cs) in state.CardSets)
        {
            var csArr = new JsonArray();
            foreach (var c in cs.Cards) csArr.Add(c);
            cardSets[name] = new JsonObject { ["cards"] = csArr };
        }
        root["cardSets"] = cardSets;

        // Phases
        var phases = new JsonArray();
        foreach (var phase in state.Phases)
        {
            var phObj = new JsonObject();
            phObj["name"] = phase.Name;
            if (phase.InitDsl is not null)    phObj["init"]    = phase.InitDsl;
            if (phase.CleanupDsl is not null) phObj["cleanup"] = phase.CleanupDsl;
            phases.Add(phObj);
        }
        root["phases"] = phases;

        // Action rules
        var actionRules = new JsonObject();
        foreach (var (actionType, rules) in state.ActionRules)
        {
            var ruleArr = new JsonArray();
            foreach (var rule in rules)
            {
                var rObj = new JsonObject();
                if (rule.BeforeDsl is not null) rObj["before"] = rule.BeforeDsl;
                if (rule.AfterDsl is not null)  rObj["after"]  = rule.AfterDsl;
                ruleArr.Add(rObj);
            }
            actionRules[actionType] = ruleArr;
        }
        root["actionRules"] = actionRules;

        // State-based rules
        var sbrs = new JsonArray();
        foreach (var sbr in state.StateBasedRules)
        {
            sbrs.Add(new JsonObject
            {
                ["name"]      = sbr.Name,
                ["condition"] = sbr.ConditionDsl,
                ["body"]      = sbr.BodyDsl,
            });
        }
        root["stateBasedRules"] = sbrs;

        // Init manifest
        if (state.InitManifest is { } im)
            root["initManifest"] = SerializeInitManifest(im);

        // Playable zone names
        if (state.PlayableZoneNames.Count > 0)
        {
            var pzn = new JsonArray();
            foreach (var z in state.PlayableZoneNames) pzn.Add(z);
            root["playableZoneNames"] = pzn;
        }

        // Localization
        var locObj = new JsonObject();
        locObj["sourceLanguage"] = state.Localization.SourceLanguage;
        var stringsObj = new JsonObject();
        foreach (var (locale, table) in state.Localization.Strings)
        {
            var localeObj = new JsonObject();
            foreach (var (k, v) in table) localeObj[k] = v;
            stringsObj[locale] = localeObj;
        }
        locObj["strings"] = stringsObj;
        root["localization"] = locObj;

        // Tooling section — round-trip verbatim
        if (state.EditorState.HasValue)
        {
            var tooling = new JsonObject();
            tooling["editorState"] = JsonNode.Parse(state.EditorState.Value.GetRawText());
            root["tooling"] = tooling;
        }

        return root.ToJsonString(_writeOpts);
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static JsonArray SerializeCostEntries(List<CostEntry> costs)
    {
        var arr = new JsonArray();
        foreach (var cost in costs)
        {
            var c = new JsonObject { ["body"] = cost.BodyDsl };
            if (cost.TextTemplate is not null) c["textTemplate"] = cost.TextTemplate;
            arr.Add(c);
        }
        return arr;
    }

    private static JsonArray SerializeStaticEffectEntries(List<StaticEffectEntry> effects)
    {
        var arr = new JsonArray();
        foreach (var se in effects)
        {
            var seObj = new JsonObject { ["contribution"] = se.ContributionDsl };
            if (se.TriggerConditionDsl is not null)
                seObj["triggerCondition"] = se.TriggerConditionDsl;
            if (se.TriggerEventKeyword is not null)
                seObj["triggerEventKeyword"] = se.TriggerEventKeyword;
            if (se.TriggerScope != TriggerScope.ThisAction)
                seObj["triggerScope"] = se.TriggerScope.ToString();
            if (se.TriggerBodyDsl is not null)
                seObj["triggerBody"] = se.TriggerBodyDsl;
            // D27: LifetimeDsl is the canonical form; use the DSL string directly.
            if (se.LifetimeDsl is not null)
                seObj["lifetime"] = se.LifetimeDsl;
            arr.Add(seObj);
        }
        return arr;
    }

    private static JsonObject SerializeInitManifest(InitManifestEntry im)
    {
        var obj = new JsonObject();

        var zones = new JsonArray();
        foreach (var z in im.Zones)
        {
            // D27: "definition" must hold the zone definition name (key from
            // GameDefinition.ZoneDefinitions), NOT the LocalId.
            var zObj = new JsonObject
            {
                ["owner"]      = z.Owner,
                ["definition"] = z.Definition,
                ["localId"]    = z.LocalId,
            };
            zones.Add(zObj);
        }
        obj["zones"] = zones;

        var cards = new JsonArray();
        foreach (var c in im.Cards)
        {
            var cObj = new JsonObject
            {
                ["owner"]       = c.Owner,
                ["zoneLocalId"] = c.ZoneLocalId,
                ["definition"]  = c.Definition,
            };
            if (c.LocalId is not null) cObj["localId"] = c.LocalId;
            cards.Add(cObj);
        }
        obj["cards"] = cards;

        var ps = new JsonArray();
        foreach (var p in im.Players)
            ps.Add(new JsonObject { ["player"] = p.Player });
        obj["players"] = ps;

        return obj;
    }
}
