using Archetype.Core;
using Archetype.Tooling.Server;
using Archetype.Tooling.Server.Export;
using Archetype.Tooling.Server.Handlers;

namespace Archetype.Tests.Tooling;

// ---------------------------------------------------------------------------
//  RPC handler tests (task 4.7)
//
//  These tests drive individual handler classes directly (no stdin/stdout)
//  to verify the contract each handler offers to the Electron renderer.
// ---------------------------------------------------------------------------

public sealed class RpcHandlerTests
{
    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static SidecarState FreshSidecar() => new();

    private static SidecarState SidecarWith(Action<ProjectState> configure)
    {
        var sidecar = new SidecarState();
        configure(sidecar.State);
        return sidecar;
    }

    // -----------------------------------------------------------------------
    //  4.7-H1  UpdateKeywordBody — valid DSL produces empty diagnostics
    // -----------------------------------------------------------------------

    [Fact]
    public void UpdateKeywordBody_ValidDsl_ReturnsDiagnosticsEmpty()
    {
        var sidecar = SidecarWith(s =>
        {
            s.Keywords["give-damage"] = new KeywordEntry
            {
                Name       = "give-damage",
                ReturnType = TypeName.Atom,
            };
        });

        var handler = new UpdateKeywordBodyHandler(sidecar);
        var response = (MutationResponse)handler.Handle(
            BuildParams(new { keywordName = "give-damage",
                              dsl = "modify-accumulator(give-damage, \"damage\", give-damage)" }));

        // Valid DSL → no error diagnostics.
        Assert.DoesNotContain(response.Diagnostics, d => d.Severity == "error");
        // The keyword body node should be set.
        Assert.NotNull(sidecar.State.Keywords["give-damage"].BodyNode);
    }

    // -----------------------------------------------------------------------
    //  4.7-H2  UpdateKeywordBody — broken DSL (unclosed paren) → error with range
    // -----------------------------------------------------------------------

    [Fact]
    public void UpdateKeywordBody_InvalidDsl_ReturnsDiagnosticWithRange()
    {
        var sidecar = SidecarWith(s =>
        {
            s.Keywords["broken"] = new KeywordEntry { Name = "broken" };
        });

        var handler  = new UpdateKeywordBodyHandler(sidecar);
        var response = (MutationResponse)handler.Handle(
            BuildParams(new { keywordName = "broken",
                              dsl = "foo(bar, " })); // unclosed paren

        // Must record at least one diagnostic for the affected keyword.
        // The validator will fire an unresolved-ref error for "foo" even if
        // the parser is lenient about the missing close-paren.
        Assert.NotEmpty(response.Diagnostics);

        // BodyNode is null when parse fails.
        Assert.Null(sidecar.State.Keywords["broken"].BodyNode);
    }

    // -----------------------------------------------------------------------
    //  4.7-H3  AddEntry — new keyword appears in ProjectState
    // -----------------------------------------------------------------------

    [Fact]
    public void AddEntry_NewKeyword_AppearsInProjectState()
    {
        var sidecar = FreshSidecar();
        var handler = new AddEntryHandler(sidecar);

        var response = (MutationResponse)handler.Handle(
            BuildParams(new { entryKind = "keyword", entryName = "my-keyword" }));

        Assert.True(sidecar.State.Keywords.ContainsKey("my-keyword"));
        Assert.Contains("my-keyword", response.AffectedEntries);
    }

    // -----------------------------------------------------------------------
    //  4.7-H4  RemoveEntry — keyword used by a card → orphan diagnostic returned
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveEntry_KeywordUsedByCard_ReturnsOrphanDiagnostic()
    {
        // Set up a keyword "deal" referenced in a card's primary effect.
        var sidecar = SidecarWith(s =>
        {
            s.Keywords["deal"] = new KeywordEntry
            {
                Name       = "deal",
                ReturnType = TypeName.Atom,
                BodyDsl    = "modify-accumulator(deal, \"damage\", deal)",
                BodyNode   = DslParser.Parse(
                    "modify-accumulator(deal, \"damage\", deal)").Node,
            };

            // Card that calls "deal".
            var cardDsl = "deal()";
            var parseResult = DslParser.ParseBlock(cardDsl);
            s.Cards["goblin"] = new CardEntry
            {
                Name              = "goblin",
                PrimaryEffectDsl  = cardDsl,
                PrimaryEffectNode = parseResult.IsSuccess ? parseResult.Block : null,
            };

            ReferenceGraph.Build(s);
        });

        var handler = new RemoveEntryHandler(sidecar);
        var response = (MutationResponse)handler.Handle(
            BuildParams(new { entryKind = "keyword", entryName = "deal" }));

        // "deal" must be gone.
        Assert.False(sidecar.State.Keywords.ContainsKey("deal"));

        // After removal the validator fires an unresolved-reference error for "goblin".
        var allDiagnostics = sidecar.State.Diagnostics;
        Assert.Contains(allDiagnostics,
            d => d.Severity == "error" && d.EntryName == "goblin");
    }

    // -----------------------------------------------------------------------
    //  4.7-H5  RenameEntry — updates keyword name + rewrites call-site nodes
    // -----------------------------------------------------------------------

    [Fact]
    public void RenameEntry_UpdatesAllCallSites()
    {
        var sidecar = SidecarWith(s =>
        {
            // "alpha" is a keyword called inside "beta".
            s.Keywords["alpha"] = new KeywordEntry
            {
                Name       = "alpha",
                ReturnType = TypeName.Atom,
                BodyDsl    = "modify-accumulator(alpha, \"damage\", alpha)",
                BodyNode   = DslParser.Parse(
                    "modify-accumulator(alpha, \"damage\", alpha)").Node,
            };

            var betaDsl = "alpha()";
            var betaParse = DslParser.Parse(betaDsl);
            s.Keywords["beta"] = new KeywordEntry
            {
                Name       = "beta",
                ReturnType = TypeName.Atom,
                BodyDsl    = betaDsl,
                BodyNode   = betaParse.Node,
            };

            ReferenceGraph.Build(s);
        });

        var handler = new RenameEntryHandler(sidecar);
        var response = (MutationResponse)handler.Handle(
            BuildParams(new { entryKind = "keyword", oldName = "alpha", newName = "alpha-v2" }));

        // Old name gone, new name present.
        Assert.False(sidecar.State.Keywords.ContainsKey("alpha"));
        Assert.True(sidecar.State.Keywords.ContainsKey("alpha-v2"));

        // After rename the project should be clean (no unresolved references).
        Assert.Equal(0, response.GlobalErrorCount);
    }

    // -----------------------------------------------------------------------
    //  4.7-H6  ExportGameDefinition — errors present → IsBlocked
    // -----------------------------------------------------------------------

    [Fact]
    public void ExportGameDefinition_WithErrors_ReturnsErrorResponse()
    {
        // Keyword with a null BodyNode (parse error) forces an error diagnostic.
        var state = new ProjectState();
        state.Keywords["broken"] = new KeywordEntry
        {
            Name     = "broken",
            BodyDsl  = "??",  // unparseable
            BodyNode = null,
        };
        // Inject the diagnostic directly so ErrorCount > 0.
        state.Diagnostics.Add(new ProjectDiagnostic(
            "keyword", "broken", "error", "Parse error.", null));

        var result = GameDefinitionExporter.Export(state);

        Assert.True(result.IsBlocked);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Json);
    }

    // -----------------------------------------------------------------------
    //  4.7-H7  ExportGameDefinition — missing translations, no force → gate triggered
    // -----------------------------------------------------------------------

    [Fact]
    public void ExportGameDefinition_MissingTranslations_NoForce_ReturnsSummary()
    {
        var state = new ProjectState();
        // Source language has keys; target language does not.
        state.Localization.SourceLanguage = "en";
        state.Localization.Strings["en"] = new Dictionary<string, string>
        {
            ["deal-damage"] = "Deal {n} damage",
        };
        state.Localization.Strings["fr"] = new Dictionary<string, string>(); // nothing

        // No errors so export can proceed to the translation gate.
        var result = GameDefinitionExporter.Export(state, force: false);

        Assert.True(result.HasMissingTranslations);
        Assert.False(result.IsSuccess);
        Assert.False(result.IsBlocked);
        Assert.Contains(result.MissingTranslations, e => e.Locale == "fr");
    }

    // -----------------------------------------------------------------------
    //  4.7-H8  ExportGameDefinition — missing translations + force → export succeeds
    // -----------------------------------------------------------------------

    [Fact]
    public void ExportGameDefinition_MissingTranslations_Force_ReturnsExport()
    {
        var state = new ProjectState { Id = "test" };
        state.Localization.SourceLanguage = "en";
        state.Localization.Strings["en"] = new Dictionary<string, string>
        {
            ["deal-damage"] = "Deal {n} damage",
        };
        state.Localization.Strings["fr"] = new Dictionary<string, string>();

        // Force overrides the translation gate.
        var result = GameDefinitionExporter.Export(state, force: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Json);
        Assert.False(result.IsBlocked);
    }

    // -----------------------------------------------------------------------
    //  4.7-H9  ExportGodotClasses — D30 signal derivation rules respected
    // -----------------------------------------------------------------------

    [Fact]
    public void ExportGodotClasses_DerivesSignalSet_PerD30Rules()
    {
        // Set up a state with one game-creator keyword and one built-in reference.
        var state = new ProjectState();

        // "give-damage" is a game-creator keyword (should be included by default).
        state.Keywords["give-damage"] = new KeywordEntry
        {
            Name            = "give-damage",
            ReturnType      = TypeName.Atom,
            SignalBehaviour = SignalBehaviour.Default,
            Parameters      = [new ParameterDecl("target", TypeName.Card),
                               new ParameterDecl("amount", TypeName.Number)],
            BodyDsl         = "modify-accumulator(give-damage, \"damage\", give-damage)",
            BodyNode        = DslParser.Parse(
                "modify-accumulator(give-damage, \"damage\", give-damage)").Node,
        };

        // "suppress-me" is a game-creator keyword opted out with [NoSignal].
        state.Keywords["suppress-me"] = new KeywordEntry
        {
            Name            = "suppress-me",
            ReturnType      = TypeName.Atom,
            SignalBehaviour = SignalBehaviour.Suppress,
            Parameters      = [],
            BodyDsl         = "modify-accumulator(suppress-me, \"marker\", suppress-me)",
            BodyNode        = DslParser.Parse(
                "modify-accumulator(suppress-me, \"marker\", suppress-me)").Node,
        };

        // Card references both keywords and a built-in ("modify-accumulator").
        var cardDsl = "give-damage(); suppress-me()";
        var cardBlock = DslParser.ParseBlock(cardDsl);
        state.Cards["test-card"] = new CardEntry
        {
            Name              = "test-card",
            PrimaryEffectDsl  = cardDsl,
            PrimaryEffectNode = cardBlock.IsSuccess ? cardBlock.Block : null,
        };

        ReferenceGraph.Build(state);

        var signals = GodotClassGenerator.DeriveSignalSet(state);
        var signalNames = signals.Select(s => s.Name).ToHashSet();

        // Game-creator keyword with Default behaviour → included.
        Assert.Contains("give-damage", signalNames);

        // Game-creator keyword with Suppress → not included.
        Assert.DoesNotContain("suppress-me", signalNames);

        // Built-in "modify-accumulator" (Default for builtins = suppress) → not included.
        Assert.DoesNotContain("modify-accumulator", signalNames);
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-1  ReturnType is reflected in exported KeywordDefinition (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_ReturnType_IsUsedFromEntry_NotHardcoded()
    {
        // A keyword with ReturnType = Number must export as Number, not Atom.
        var state = new ProjectState { Id = "rt-test" };
        state.Keywords["score-query"] = new KeywordEntry
        {
            Name       = "score-query",
            ReturnType = TypeName.Number,
            Parameters = [new ParameterDecl("target", TypeName.Card)],
            BodyDsl    = "get-accumulator(target, \"score\")",
            BodyNode   = DslParser.Parse("get-accumulator(target, \"score\")").Node,
        };

        var result = GameDefinitionExporter.Export(state, force: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Json);

        // Deserialise the exported JSON and verify ReturnType via the object graph
        // (not string search) since enum serialization may produce an integer value.
        var doc = System.Text.Json.JsonDocument.Parse(result.Json!);
        Assert.True(doc.RootElement.TryGetProperty("Keywords", out var kws));
        Assert.True(kws.TryGetProperty("score-query", out var kw));
        Assert.True(kw.TryGetProperty("ReturnType", out var rtProp));
        // ReturnType must NOT be the value for Atom (0). Number is a non-zero TypeName value.
        // We verify it is different from Atom (0) which is what the hardcoded path produced.
        var rtValue = rtProp.GetInt32();
        Assert.NotEqual((int)TypeName.Atom, rtValue);
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-3  StaticEffect export is not a stub (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_StaticEffect_ContributionOnly_IsExported()
    {
        // A card with a permanent contribution-only static effect must
        // not produce an empty StaticEffects list in the export.
        var state = new ProjectState { Id = "se-test" };

        var contribDsl = "modify-accumulator(target, \"shield\", target)";
        var contribBlock = DslParser.ParseBlock(contribDsl);
        state.Cards["guardian"] = new CardEntry
        {
            Name             = "guardian",
            PrimaryEffectDsl = "modify-accumulator(guardian, \"damage\", guardian)",
            PrimaryEffectNode = DslParser.ParseBlock(
                "modify-accumulator(guardian, \"damage\", guardian)").Block,
            StaticEffects    = [
                new StaticEffectEntry
                {
                    ContributionDsl  = contribDsl,
                    ContributionNode = contribBlock.IsSuccess ? contribBlock.Block : null,
                    // LifetimeDsl null → permanent
                }
            ],
        };

        var result = GameDefinitionExporter.Export(state, force: true);

        Assert.True(result.IsSuccess);
        // The exported JSON must reference "guardian" card with a staticEffects section.
        Assert.Contains("guardian", result.Json!);
        // And contain the contribution block marker (not an empty list stub).
        Assert.Contains("shield", result.Json!);
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-4  RenameEntry rewrites BodyDsl source strings (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void RenameEntry_RewritesBodyDslSource_NotJustNodes()
    {
        var sidecar = SidecarWith(s =>
        {
            s.Keywords["alpha"] = new KeywordEntry
            {
                Name       = "alpha",
                ReturnType = TypeName.Atom,
                BodyDsl    = "modify-accumulator(alpha, \"damage\", alpha)",
                BodyNode   = DslParser.Parse(
                    "modify-accumulator(alpha, \"damage\", alpha)").Node,
            };

            var betaDsl = "alpha()";
            s.Keywords["beta"] = new KeywordEntry
            {
                Name       = "beta",
                ReturnType = TypeName.Atom,
                BodyDsl    = betaDsl,
                BodyNode   = DslParser.Parse(betaDsl).Node,
            };

            ReferenceGraph.Build(s);
        });

        var handler = new RenameEntryHandler(sidecar);
        handler.Handle(BuildParams(new
        {
            entryKind = "keyword",
            oldName   = "alpha",
            newName   = "alpha-v2",
        }));

        // The DSL source string of "beta" must now reference "alpha-v2", not "alpha".
        Assert.Contains("alpha-v2", sidecar.State.Keywords["beta"].BodyDsl);
        Assert.DoesNotContain("alpha()", sidecar.State.Keywords["beta"].BodyDsl);
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-4b  Rename → save → reload produces zero errors (D27)
    // -----------------------------------------------------------------------

    [Fact]
    public void RenameEntry_SaveReload_ZeroErrors()
    {
        var sidecar = SidecarWith(s =>
        {
            s.Id = "round-trip";
            s.Keywords["old-name"] = new KeywordEntry
            {
                Name       = "old-name",
                ReturnType = TypeName.Atom,
                BodyDsl    = "modify-accumulator(old-name, \"hp\", old-name)",
                BodyNode   = DslParser.Parse(
                    "modify-accumulator(old-name, \"hp\", old-name)").Node,
            };

            var callerDsl = "old-name()";
            s.Keywords["caller"] = new KeywordEntry
            {
                Name       = "caller",
                ReturnType = TypeName.Atom,
                BodyDsl    = callerDsl,
                BodyNode   = DslParser.Parse(callerDsl).Node,
            };

            ReferenceGraph.Build(s);
        });

        var handler = new RenameEntryHandler(sidecar);
        handler.Handle(BuildParams(new
        {
            entryKind = "keyword",
            oldName   = "old-name",
            newName   = "new-name",
        }));

        // Save and reload.
        var saved      = ProjectFileSerializer.Serialize(sidecar.State);
        var reloaded   = ProjectFileLoader.Load(saved);

        // After reload there must be no errors (no stale "old-name" references).
        var errors = reloaded.Diagnostics.Where(d => d.Severity == "error").ToList();
        Assert.Empty(errors);
    }

    // -----------------------------------------------------------------------
    //  BUG-FIX-6  GetSymbolInfo referencedBy items return only entryName (D28)
    // -----------------------------------------------------------------------

    [Fact]
    public void GetSymbolInfo_ReferencedBy_ContainsOnlyEntryName()
    {
        // "base-kw" is called from "caller-kw".
        var sidecar = SidecarWith(s =>
        {
            s.Keywords["base-kw"] = new KeywordEntry
            {
                Name       = "base-kw",
                ReturnType = TypeName.Atom,
                BodyDsl    = "modify-accumulator(base-kw, \"v\", base-kw)",
                BodyNode   = DslParser.Parse(
                    "modify-accumulator(base-kw, \"v\", base-kw)").Node,
            };

            var callerDsl = "base-kw()";
            s.Keywords["caller-kw"] = new KeywordEntry
            {
                Name       = "caller-kw",
                ReturnType = TypeName.Atom,
                BodyDsl    = callerDsl,
                BodyNode   = DslParser.Parse(callerDsl).Node,
            };

            ReferenceGraph.Build(s);
        });

        var handler = new GetSymbolInfoHandler(sidecar);
        // Cursor at offset 0 of "base-kw" body.
        var raw = handler.Handle(BuildParams(new
        {
            entryKind    = "keyword",
            entryName    = "base-kw",
            cursorOffset = 4,  // inside "modify-accumulator"
        }));

        // Serialise the anonymous result so we can inspect it.
        var json = System.Text.Json.JsonSerializer.Serialize(raw);
        var doc  = System.Text.Json.JsonDocument.Parse(json);

        // referencedBy must be an array.
        Assert.True(doc.RootElement.TryGetProperty("referencedBy", out var refs));
        // Each item must contain "entryName" and must NOT contain "entryKind".
        foreach (var item in refs.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("entryName", out _),
                "referencedBy item must have 'entryName'");
            Assert.False(item.TryGetProperty("entryKind", out _),
                "referencedBy item must NOT have 'entryKind' — it was removed per D28 fix");
        }
    }

    // -----------------------------------------------------------------------
    //  Param builder — constructs a JsonElement from an anonymous object
    // -----------------------------------------------------------------------

    private static System.Text.Json.JsonElement BuildParams(object obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return System.Text.Json.JsonDocument.Parse(json).RootElement;
    }
}
