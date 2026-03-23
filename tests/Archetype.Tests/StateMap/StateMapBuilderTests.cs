using Archetype.Build;
using Archetype.Core;

namespace Archetype.Tests.StateMap;

public class StateMapBuilderTests
{
    private static readonly IReadOnlyDictionary<string, object> NoProps =
        new Dictionary<string, object>();

    private static GameDefinitionBuilder MinimalBuilder() =>
        new GameDefinitionBuilder()
            .WithId("test-game")
            .WithInitManifest(InitManifest.Empty);

    // -----------------------------------------------------------------------
    //  AddZone
    // -----------------------------------------------------------------------

    [Fact]
    public void AddZone_WithDeclarations_StoredOnDefinition()
    {
        var decls = new List<StateFieldDecl>
        {
            new("capacity", StateFieldType.Number),
        };

        var def = MinimalBuilder()
            .AddZone("hand", NoProps, decls)
            .Build();

        var zoneDef = def.ZoneDefinitions["hand"];
        Assert.NotNull(zoneDef.StateMapDeclarations);
        Assert.Single(zoneDef.StateMapDeclarations);
        Assert.Equal("capacity", zoneDef.StateMapDeclarations![0].Name);
        Assert.Equal(StateFieldType.Number, zoneDef.StateMapDeclarations![0].FieldType);
    }

    [Fact]
    public void AddZone_WithoutDeclarations_EmptyList()
    {
        var def = MinimalBuilder()
            .AddZone("hand", NoProps)
            .Build();

        var zoneDef = def.ZoneDefinitions["hand"];
        // null is the stored value when no declarations are given;
        // callers should treat null as empty (per spec).
        Assert.Null(zoneDef.StateMapDeclarations);
    }

    // -----------------------------------------------------------------------
    //  AddPlayer
    // -----------------------------------------------------------------------

    [Fact]
    public void AddPlayer_WithDeclarations_StoredOnDefinition()
    {
        var decls = new List<StateFieldDecl>
        {
            new("mana", StateFieldType.Number),
            new("tapped", StateFieldType.Bool),
        };

        var def = MinimalBuilder()
            .AddPlayer("player1", NoProps, decls)
            .Build();

        var playerDef = def.PlayerDefinitions["player1"];
        Assert.NotNull(playerDef.StateMapDeclarations);
        Assert.Equal(2, playerDef.StateMapDeclarations!.Count);
    }

    // -----------------------------------------------------------------------
    //  WithSessionStateMap
    // -----------------------------------------------------------------------

    [Fact]
    public void WithSessionStateMap_StoredOnGameDefinition()
    {
        var decls = new List<StateFieldDecl>
        {
            new("active-player", StateFieldType.Number),
        };

        var def = MinimalBuilder()
            .WithSessionStateMap(decls)
            .Build();

        Assert.NotNull(def.SessionStateMapDeclarations);
        Assert.Single(def.SessionStateMapDeclarations!);
        Assert.Equal("active-player", def.SessionStateMapDeclarations![0].Name);
    }

    // -----------------------------------------------------------------------
    //  AddCard via record constructor
    // -----------------------------------------------------------------------

    [Fact]
    public void AddCard_WithDeclarations_StoredViaRecord()
    {
        var decls = new List<StateFieldDecl>
        {
            new("health", StateFieldType.Number, "current health"),
            new("poisoned", StateFieldType.Bool),
        };

        var card = new CardDefinition(
            Name: "TestCard",
            StaticProperties: NoProps,
            PrimaryEffect: EffectBlockDef.Empty,
            AdditionalEffects: [],
            StaticEffects: [],
            StateMapDeclarations: decls);

        var def = MinimalBuilder()
            .AddCard(card)
            .Build();

        var cardDef = def.CardDefinitions["TestCard"];
        Assert.NotNull(cardDef.StateMapDeclarations);
        Assert.Equal(2, cardDef.StateMapDeclarations!.Count);
        Assert.Equal("health", cardDef.StateMapDeclarations![0].Name);
        Assert.Equal("current health", cardDef.StateMapDeclarations![0].TextTemplate);
    }
}
