using Archetype.Build;
using Archetype.Build.Extensions;
using Archetype.Core;

namespace Archetype.Tests.Builder;

public class CardSetTests
{
    private static GameDefinition BaseDefinition() =>
        new GameDefinitionBuilder()
            .WithId("test-game")
            .RegisterKeyword(new KeywordDefinitionBuilder("deal-damage")
                .WithParam("target", TypeName.Card)
                .WithParam("amount", TypeName.Number)
                .WithBody(Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Multiply(Kw.Param("amount"), Kw.Num(-1))))
                .WithTextTemplate("Deal {amount} damage to {target}")
                .Build())
            .WithInitManifest(InitManifest.Empty)
            .Build();

    private static CardDefinition MakeCard(string name, string keyword = "deal-damage") =>
        new CardDefinition(
            Name: name,
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: new EffectBlockDef([
                new EffectBlockStep(keyword, [Kw.Param("source"), Kw.Num(3)]),
            ]),
            AdditionalEffects: [],
            StaticEffects: []);

    // -----------------------------------------------------------------------
    //  Successful merge
    // -----------------------------------------------------------------------

    [Fact]
    public void WithCardSets_SingleSet_AddsCardsToDefinition()
    {
        var def = BaseDefinition();
        var set = new CardSet("core", 1, [MakeCard("FireBolt"), MakeCard("IceShard")]);

        var merged = def.WithCardSets([set]);

        Assert.True(merged.CardDefinitions.ContainsKey("FireBolt"));
        Assert.True(merged.CardDefinitions.ContainsKey("IceShard"));
    }

    [Fact]
    public void WithCardSets_MultipleSets_AllCardsPresent()
    {
        var def = BaseDefinition();
        var core = new CardSet("core", 1, [MakeCard("FireBolt")]);
        var expansion = new CardSet("expansion-1", 1, [MakeCard("IceShard")]);

        var merged = def.WithCardSets([core, expansion]);

        Assert.True(merged.CardDefinitions.ContainsKey("FireBolt"));
        Assert.True(merged.CardDefinitions.ContainsKey("IceShard"));
    }

    [Fact]
    public void WithCardSets_DoesNotMutateOriginalDefinition()
    {
        var def = BaseDefinition();
        var set = new CardSet("core", 1, [MakeCard("FireBolt")]);

        def.WithCardSets([set]);

        Assert.False(def.CardDefinitions.ContainsKey("FireBolt"));
    }

    [Fact]
    public void WithCardSets_EmptySet_ReturnsEquivalentDefinition()
    {
        var def = BaseDefinition();
        var merged = def.WithCardSets([new CardSet("empty", 1, [])]);

        Assert.Equal(def.CardDefinitions.Count, merged.CardDefinitions.Count);
    }

    // -----------------------------------------------------------------------
    //  Duplicate card name detection
    // -----------------------------------------------------------------------

    [Fact]
    public void WithCardSets_DuplicateCardNameAcrossSets_ThrowsDefinitionException()
    {
        var def = BaseDefinition();
        var setA = new CardSet("core", 1, [MakeCard("FireBolt")]);
        var setB = new CardSet("expansion", 1, [MakeCard("FireBolt")]);

        var ex = Assert.Throws<DefinitionException>(() => def.WithCardSets([setA, setB]));
        Assert.Contains("FireBolt", ex.Message);
    }

    [Fact]
    public void WithCardSets_CardNameAlreadyInBaseDefinition_ThrowsDefinitionException()
    {
        var def = BaseDefinition().WithCardSets([new CardSet("core", 1, [MakeCard("FireBolt")])]);
        var set2 = new CardSet("expansion", 1, [MakeCard("FireBolt")]);

        var ex = Assert.Throws<DefinitionException>(() => def.WithCardSets([set2]));
        Assert.Contains("FireBolt", ex.Message);
    }

    // -----------------------------------------------------------------------
    //  Keyword reference validation
    // -----------------------------------------------------------------------

    [Fact]
    public void WithCardSets_UnknownKeywordReference_ThrowsDefinitionException()
    {
        var def = BaseDefinition();
        var set = new CardSet("core", 1, [MakeCard("BadCard", keyword: "nonexistent-keyword")]);

        var ex = Assert.Throws<DefinitionException>(() => def.WithCardSets([set]));
        Assert.Contains("nonexistent-keyword", ex.Message);
        Assert.Contains("BadCard", ex.Message);
    }

    [Fact]
    public void WithCardSets_ValidKeywordReference_Succeeds()
    {
        var def = BaseDefinition();
        var set = new CardSet("core", 1, [MakeCard("GoodCard", keyword: "deal-damage")]);

        var merged = def.WithCardSets([set]);

        Assert.True(merged.CardDefinitions.ContainsKey("GoodCard"));
    }
}
