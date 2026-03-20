using Archetype.Build;
using Archetype.Core;

namespace Archetype.Tests.Builder;

public class BuildRunnerTests : IDisposable
{
    private readonly string _outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, recursive: true);
    }

    private static GameDefinition BaseDefinition() =>
        new GameDefinitionBuilder()
            .WithId("test-game")
            .RegisterKeyword(
                name: "strike",
                parameters: [new ParameterDecl("target", TypeName.Card), new ParameterDecl("power", TypeName.Number)],
                body: Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Multiply(Kw.Param("power"), Kw.Num(-1))),
                textTemplate: "{target} takes {power} damage")
            .RegisterKeyword(
                name: "buff",
                parameters: [new ParameterDecl("target", TypeName.Card), new ParameterDecl("amount", TypeName.Number)],
                body: Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("attack"), Kw.Param("amount")),
                textTemplate: "Give {target} +{amount} attack")
            .WithInitManifest(InitManifest.Empty)
            .Build();

    private static CardSet MakeSet(string name, params string[] keywords) =>
        new CardSet(name, 1, keywords.Select((kw, i) => new CardDefinition(
            Name: $"{name}-Card{i}",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: new EffectBlockDef([new EffectBlockStep(kw, [Kw.Param("source"), Kw.Num(1)])]),
            AdditionalEffects: [],
            StaticEffects: [])).ToList());

    // -----------------------------------------------------------------------
    //  Output directory creation
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_CreatesOutputDirectoryIfNotExists()
    {
        var dir = Path.Combine(_outputDir, "new-subdir");
        BuildRunner.Run(BaseDefinition(), [], dir);
        Assert.True(Directory.Exists(dir));
    }

    [Fact]
    public void Run_SucceedsWhenOutputDirectoryAlreadyExists()
    {
        Directory.CreateDirectory(_outputDir);
        BuildRunner.Run(BaseDefinition(), [], _outputDir); // Should not throw.
        Assert.True(Directory.Exists(_outputDir));
    }

    // -----------------------------------------------------------------------
    //  Card set JSON output
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_WritesOneJsonFilePerCardSet()
    {
        var sets = new[]
        {
            MakeSet("core", "strike"),
            MakeSet("expansion-1", "buff"),
        };

        BuildRunner.Run(BaseDefinition(), sets, _outputDir);

        Assert.True(File.Exists(Path.Combine(_outputDir, "core.json")));
        Assert.True(File.Exists(Path.Combine(_outputDir, "expansion-1.json")));
    }

    [Fact]
    public void Run_OverwritesExistingCardSetFile()
    {
        var setPath = Path.Combine(_outputDir, "core.json");
        Directory.CreateDirectory(_outputDir);
        File.WriteAllText(setPath, "old content");

        BuildRunner.Run(BaseDefinition(), [MakeSet("core", "strike")], _outputDir);

        var content = File.ReadAllText(setPath);
        Assert.NotEqual("old content", content);
        Assert.Contains("core-Card0", content);
    }

    [Fact]
    public void Run_NoSets_NoJsonFilesWritten()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);

        var jsonFiles = Directory.GetFiles(_outputDir, "*.json");
        Assert.Empty(jsonFiles);
    }

    // -----------------------------------------------------------------------
    //  Keyword constants file
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_EmitsKeywordConstantsFile()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        Assert.True(File.Exists(Path.Combine(_outputDir, "archetype_keywords.gd")));
    }

    [Fact]
    public void Run_KeywordConstantsFile_ContainsGameCreatorKeywords()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "archetype_keywords.gd"));
        Assert.Contains("STRIKE = \"strike\"", content);
        Assert.Contains("BUFF = \"buff\"", content);
    }

    [Fact]
    public void Run_KeywordConstantsFile_ContainsBuiltInKeywords()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "archetype_keywords.gd"));
        Assert.Contains("MODIFY_ACCUMULATOR = \"modify-accumulator\"", content);
    }

    // -----------------------------------------------------------------------
    //  Signal definitions file
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_EmitsSignalDefinitionsFile()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        Assert.True(File.Exists(Path.Combine(_outputDir, "game_events.gd")));
    }

    [Fact]
    public void Run_SignalsFile_EmitsSignalForReferencedKeyword()
    {
        var set = MakeSet("core", "strike");
        BuildRunner.Run(BaseDefinition(), [set], _outputDir);

        var content = File.ReadAllText(Path.Combine(_outputDir, "game_events.gd"));
        Assert.Contains("signal on_strike(", content);
    }

    [Fact]
    public void Run_SignalsFile_SuppressesBuiltInKeywords()
    {
        // Create a card that directly uses a built-in step.
        var card = new CardDefinition(
            Name: "TestCard",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: new EffectBlockDef([
                new EffectBlockStep("modify-accumulator", [Kw.Param("source"), Kw.Str("hp"), Kw.Num(-1)]),
            ]),
            AdditionalEffects: [],
            StaticEffects: []);
        var set = new CardSet("core", 1, [card]);

        BuildRunner.Run(BaseDefinition(), [set], _outputDir);

        var content = File.ReadAllText(Path.Combine(_outputDir, "game_events.gd"));
        Assert.DoesNotContain("on_modify_accumulator", content);
    }

    [Fact]
    public void Run_SignalsFile_ExcludesOptedOutKeyword()
    {
        var set = MakeSet("core", "strike", "buff");
        BuildRunner.Run(BaseDefinition(), [set], _outputDir, noSignalKeywords: ["buff"]);

        var content = File.ReadAllText(Path.Combine(_outputDir, "game_events.gd"));
        Assert.Contains("on_strike", content);
        Assert.DoesNotContain("on_buff", content);
    }

    [Fact]
    public void Run_SignalsFile_UnreferencedKeywordProducesNoSignal()
    {
        // "buff" keyword is registered but not used in any card.
        BuildRunner.Run(BaseDefinition(), [MakeSet("core", "strike")], _outputDir);

        var content = File.ReadAllText(Path.Combine(_outputDir, "game_events.gd"));
        Assert.DoesNotContain("on_buff", content);
    }

    // -----------------------------------------------------------------------
    //  ArchetypeNode.cs — D33
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_EmitsArchetypeNodeFile()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        Assert.True(File.Exists(Path.Combine(_outputDir, "ArchetypeNode.cs")));
    }

    [Fact]
    public void Run_ArchetypeNodeFile_ContainsLifecycleSignals()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        Assert.Contains("ActionRequestedEventHandler", content);
        Assert.Contains("ActionResolvedEventHandler", content);
        Assert.Contains("PromptRequestedEventHandler", content);
        Assert.Contains("GameOverEventHandler", content);
    }

    [Fact]
    public void Run_ArchetypeNodeFile_ContainsDerivedSignalForReferencedKeyword()
    {
        var set = MakeSet("core", "strike");
        BuildRunner.Run(BaseDefinition(), [set], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        // Derived signal handler uses PascalCase: "strike" → "OnStrikeEventHandler"
        Assert.Contains("OnStrikeEventHandler", content);
    }

    [Fact]
    public void Run_ArchetypeNodeFile_ExcludesOptedOutKeywordSignal()
    {
        var set = MakeSet("core", "strike", "buff");
        BuildRunner.Run(BaseDefinition(), [set], _outputDir, noSignalKeywords: ["buff"]);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        Assert.Contains("OnStrikeEventHandler", content);
        Assert.DoesNotContain("OnBuffEventHandler", content);
    }

    [Fact]
    public void Run_ArchetypeNodeFile_ContainsSubmissionMethods()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        Assert.Contains("SubmitPlayCard", content);
        Assert.Contains("SubmitActivateAbility", content);
        Assert.Contains("SubmitPass", content);
        Assert.Contains("SubmitPromptResponse", content);
        Assert.Contains("StartGame", content);
    }

    [Fact]
    public void Run_ArchetypeNodeFile_EmptySignalSet_HasNoKeywordSignals()
    {
        // No card sets → no derived signals; lifecycle signals should still be present.
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        Assert.Contains("ActionRequestedEventHandler", content);
        Assert.DoesNotContain("OnStrikeEventHandler", content);
        Assert.DoesNotContain("OnBuffEventHandler", content);
    }

    [Fact]
    public void Run_ArchetypeNodeFile_SuppressesBuiltInKeywordSignals()
    {
        // Built-in "modify-accumulator" should not produce a derived signal even
        // when directly referenced in a card effect block.
        var card = new CardDefinition(
            Name: "DirectBuiltIn",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: new EffectBlockDef([
                new EffectBlockStep("modify-accumulator",
                    [Kw.Param("source"), Kw.Str("hp"), Kw.Num(-1)])]),
            AdditionalEffects: [],
            StaticEffects: []);
        var set = new CardSet("core", 1, [card]);

        BuildRunner.Run(BaseDefinition(), [set], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "ArchetypeNode.cs"));

        Assert.DoesNotContain("OnModifyAccumulatorEventHandler", content);
    }

    // -----------------------------------------------------------------------
    //  archetype_signals.gd — D33
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_EmitsArchetypeSignalsFile()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        Assert.True(File.Exists(Path.Combine(_outputDir, "archetype_signals.gd")));
    }

    [Fact]
    public void Run_ArchetypeSignalsFile_ContainsConstForDerivedSignal()
    {
        var set = MakeSet("core", "strike");
        BuildRunner.Run(BaseDefinition(), [set], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "archetype_signals.gd"));

        // "strike" → signal name "on_strike" → const "ON_STRIKE"
        Assert.Contains("ON_STRIKE = \"on_strike\"", content);
    }

    [Fact]
    public void Run_ArchetypeSignalsFile_EmptySignalSet_HasNoConsts()
    {
        // No cards → no derived signals → no consts (just the comment).
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "archetype_signals.gd"));

        Assert.Contains("ArchetypeSignals", content);
        Assert.DoesNotContain("ON_STRIKE", content);
    }

    // -----------------------------------------------------------------------
    //  archetype_interop.gd — D33
    // -----------------------------------------------------------------------

    [Fact]
    public void Run_EmitsArchetypeInteropFile()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        Assert.True(File.Exists(Path.Combine(_outputDir, "archetype_interop.gd")));
    }

    [Fact]
    public void Run_ArchetypeInteropFile_ContainsRegisterAndSubmitMethods()
    {
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var content = File.ReadAllText(Path.Combine(_outputDir, "archetype_interop.gd"));

        Assert.Contains("func register(node: ArchetypeNode)", content);
        Assert.Contains("func submit_pass(", content);
        Assert.Contains("func submit_play_card(", content);
        Assert.Contains("func submit_activate_ability(", content);
        Assert.Contains("func submit_prompt_response(", content);
    }

    [Fact]
    public void Run_ArchetypeInteropFile_IsIdenticalRegardlessOfSignalSet()
    {
        // interop.gd is game-agnostic — signal set changes must not alter it.
        BuildRunner.Run(BaseDefinition(), [], _outputDir);
        var noSignals = File.ReadAllText(Path.Combine(_outputDir, "archetype_interop.gd"));

        var dir2 = Path.Combine(_outputDir, "with-signals");
        BuildRunner.Run(BaseDefinition(), [MakeSet("core", "strike")], dir2);
        var withSignals = File.ReadAllText(Path.Combine(dir2, "archetype_interop.gd"));

        Assert.Equal(noSignals, withSignals);
    }
}
