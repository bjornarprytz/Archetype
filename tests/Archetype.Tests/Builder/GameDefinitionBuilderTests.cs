using Archetype.Build;
using Archetype.Build.Extensions;
using Archetype.Core;

namespace Archetype.Tests.Builder;

public class GameDefinitionBuilderTests
{
    private static GameDefinitionBuilder MinimalBuilder() =>
        new GameDefinitionBuilder()
            .WithId("test-game")
            .WithInitManifest(InitManifest.Empty);

    // -----------------------------------------------------------------------
    //  Valid build
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_WithRequiredFields_Succeeds()
    {
        var def = MinimalBuilder().Build();
        Assert.NotNull(def);
        Assert.Equal("test-game", def.Id);
        Assert.NotEmpty(def.Keywords); // built-ins always present
    }

    [Fact]
    public void Build_IncludesAllBuiltInKeywords()
    {
        var def = MinimalBuilder().Build();
        foreach (var builtin in BuiltInKeywords.All)
            Assert.True(def.Keywords.ContainsKey(builtin.Name),
                $"Built-in keyword '{builtin.Name}' missing from GameDefinition.");
    }

    [Fact]
    public void Build_WithValidGameCreatorKeyword_Succeeds()
    {
        var def = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("heal")
                .WithParam("target", TypeName.Card)
                .WithParam("amount", TypeName.Number)
                .WithBody(Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Param("amount")))
                .WithTextTemplate("{target} heals {amount}")
                .Build())
            .Build();

        Assert.True(def.Keywords.ContainsKey("heal"));
        Assert.Equal("heal", def.Keywords["heal"].Name);
        Assert.Equal("{target} heals {amount}", def.Keywords["heal"].TextTemplate);
        Assert.False(def.Keywords["heal"].IsPrimitive);
    }

    [Fact]
    public void Build_WithZoneAndPhase_IncludesThemInDefinition()
    {
        var def = MinimalBuilder()
            .AddZone("hand", new Dictionary<string, object>())
            .AddPhase(new PhaseDefinition("main"))
            .Build();

        Assert.True(def.ZoneDefinitions.ContainsKey("hand"));
        Assert.Single(def.Phases);
        Assert.Equal("main", def.Phases[0].Name);
    }

    [Fact]
    public void Build_WithCompositeKeywordCallingGameCreatorKeyword_Succeeds()
    {
        var def = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("heal")
                .WithParam("target", TypeName.Card)
                .WithParam("amount", TypeName.Number)
                .WithBody(Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Param("amount")))
                .Build())
            .RegisterKeyword(new KeywordDefinitionBuilder("big-heal")
                .WithParam("target", TypeName.Card)
                .WithBody(Kw.Invoke("heal", Kw.Param("target"), Kw.Num(5)))
                .Build())
            .Build();

        Assert.True(def.Keywords.ContainsKey("big-heal"));
    }

    // -----------------------------------------------------------------------
    //  Validation: missing InitManifest
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_WithoutInitManifest_ThrowsDefinitionException()
    {
        var builder = new GameDefinitionBuilder().WithId("test-game");
        var ex = Assert.Throws<DefinitionException>(() => builder.Build());
        Assert.Contains("InitManifest", ex.Message);
    }

    // -----------------------------------------------------------------------
    //  Validation: duplicate keyword names
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_DuplicateGameCreatorKeyword_LastWins()
    {
        // Registering the same name twice: last registration wins (dict behaviour).
        var def = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("my-kw")
                .WithParam("x", TypeName.Number)
                .WithBody(Kw.Param("x"))
                .WithTextTemplate("first")
                .Build())
            .RegisterKeyword(new KeywordDefinitionBuilder("my-kw")
                .WithParam("x", TypeName.Number)
                .WithBody(Kw.Param("x"))
                .WithTextTemplate("second")
                .Build())
            .Build();

        Assert.Equal("second", def.Keywords["my-kw"].TextTemplate);
    }

    [Fact]
    public void Build_GameCreatorKeywordShadowsBuiltIn_ThrowsDefinitionException()
    {
        var builder = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("modify-accumulator") // built-in name
                .WithParam("x", TypeName.Number)
                .WithBody(Kw.Param("x"))
                .Build());

        var ex = Assert.Throws<DefinitionException>(() => builder.Build());
        Assert.Contains("modify-accumulator", ex.Message);
        Assert.Contains("shadows", ex.Message);
    }

    // -----------------------------------------------------------------------
    //  Validation: unknown parameter references
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_UnknownParameterInBody_ThrowsDefinitionException()
    {
        var builder = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("bad-kw")
                .WithParam("target", TypeName.Card)
                .WithBody(Kw.ModifyAccumulator(Kw.Param("typo"), Kw.Str("hp"), Kw.Num(1)))
                .Build());

        var ex = Assert.Throws<DefinitionException>(() => builder.Build());
        Assert.Contains("typo", ex.Message);
        Assert.Contains("bad-kw", ex.Message);
    }

    [Fact]
    public void Build_UnknownKeywordInBody_ThrowsDefinitionException()
    {
        var builder = MinimalBuilder()
            .RegisterKeyword(new KeywordDefinitionBuilder("bad-kw")
                .WithParam("target", TypeName.Card)
                .WithBody(Kw.Invoke("nonexistent-keyword", Kw.Param("target")))
                .Build());

        var ex = Assert.Throws<DefinitionException>(() => builder.Build());
        Assert.Contains("nonexistent-keyword", ex.Message);
    }
}
