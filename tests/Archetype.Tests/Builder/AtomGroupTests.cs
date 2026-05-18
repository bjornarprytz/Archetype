using Archetype.Build;
using Archetype.Build.Extensions;
using Archetype.Core;

namespace Archetype.Tests.Builder;

public class AtomGroupTests
{
    private static readonly CostDef DefaultCost = new(
        Body: new EffectBlockDef([new EffectBlockStep("pay-mana", [])]),
        Parameters: [],
        TextTemplate: "Pay 1 mana");

    private static GameDefinitionBuilder MinimalBuilder() =>
        new GameDefinitionBuilder()
            .WithId("test-game")
            .WithInitManifest(InitManifest.Empty);

    // -----------------------------------------------------------------------
    //  Card groups
    // -----------------------------------------------------------------------

    [Fact]
    public void RegisterCardGroup_AppliesTransformToMatchingCards()
    {
        var def = MinimalBuilder()
            .AddCard("free", cb => cb.WithPrimaryEffect(b => b.Step("noop")))
            .AddCard("costly", cb => cb
                .WithPrimaryEffect(b => b.Step("noop"))
                .AddCost(DefaultCost))
            .RegisterCardGroup(
                "default-cost",
                matcher:   card => card.Cost is null or { Count: 0 },
                transform: card => card with { Cost = [DefaultCost] })
            .Build();

        // "free" had no cost — group should have applied
        Assert.NotNull(def.CardDefinitions["free"].Cost);
        Assert.Single(def.CardDefinitions["free"].Cost!);

        // "costly" already had a cost — matcher excluded it, still one cost
        Assert.Single(def.CardDefinitions["costly"].Cost!);
    }

    [Fact]
    public void RegisterCardGroup_NonMatchingCards_AreUnaffected()
    {
        var def = MinimalBuilder()
            .AddCard(new CardDefinitionBuilder("creature")
                .WithStaticProperty("type", "creature")
                .WithPrimaryEffect(b => b.Step("noop"))
                .Build())
            .AddCard(new CardDefinitionBuilder("spell")
                .WithStaticProperty("type", "spell")
                .WithPrimaryEffect(b => b.Step("noop"))
                .Build())
            .RegisterCardGroup(
                "spell-tag",
                matcher:   card => card.StaticProperties.TryGetValue("type", out var t) && t is "spell",
                transform: card => card with
                {
                    StaticProperties = new Dictionary<string, object>(card.StaticProperties) { ["tagged"] = true }
                })
            .Build();

        Assert.False(def.CardDefinitions["creature"].StaticProperties.ContainsKey("tagged"));
        Assert.True(def.CardDefinitions["spell"].StaticProperties.TryGetValue("tagged", out var v) && v is true);
    }

    [Fact]
    public void RegisterCardGroup_PriorityOrder_LowerPriorityRunsFirst()
    {
        // priority=0 sets "counter" to 1.0, priority=1 doubles it to 2.0
        var def = MinimalBuilder()
            .AddCard("test", cb => cb.WithPrimaryEffect(b => b.Step("noop")))
            .RegisterCardGroup(
                "set-counter",
                matcher:   _ => true,
                transform: card => card with
                {
                    StaticProperties = new Dictionary<string, object>(card.StaticProperties) { ["counter"] = 1.0 }
                },
                priority: 0)
            .RegisterCardGroup(
                "double-counter",
                matcher:   card => card.StaticProperties.ContainsKey("counter"),
                transform: card => card with
                {
                    StaticProperties = new Dictionary<string, object>(card.StaticProperties)
                    {
                        ["counter"] = (double)card.StaticProperties["counter"] * 2
                    }
                },
                priority: 1)
            .Build();

        Assert.Equal(2.0, def.CardDefinitions["test"].StaticProperties["counter"]);
    }

    [Fact]
    public void RegisterCardGroup_HigherPriorityRegisteredFirst_StillRunsAfterLowerPriority()
    {
        // Register priority=1 first, priority=0 second — registration order should not affect application order
        var def = MinimalBuilder()
            .AddCard("test", cb => cb.WithPrimaryEffect(b => b.Step("noop")))
            .RegisterCardGroup(
                "double-counter",
                matcher:   card => card.StaticProperties.ContainsKey("counter"),
                transform: card => card with
                {
                    StaticProperties = new Dictionary<string, object>(card.StaticProperties)
                    {
                        ["counter"] = (double)card.StaticProperties["counter"] * 2
                    }
                },
                priority: 1)
            .RegisterCardGroup(
                "set-counter",
                matcher:   _ => true,
                transform: card => card with
                {
                    StaticProperties = new Dictionary<string, object>(card.StaticProperties) { ["counter"] = 1.0 }
                },
                priority: 0)
            .Build();

        Assert.Equal(2.0, def.CardDefinitions["test"].StaticProperties["counter"]);
    }

    [Fact]
    public void RegisterCardGroup_NoCards_DoesNotThrow()
    {
        var def = MinimalBuilder()
            .RegisterCardGroup(
                "default-cost",
                matcher:   card => card.Cost is null or { Count: 0 },
                transform: card => card with { Cost = [DefaultCost] })
            .Build();

        Assert.Empty(def.CardDefinitions);
    }

    // -----------------------------------------------------------------------
    //  Zone groups
    // -----------------------------------------------------------------------

    [Fact]
    public void RegisterZoneGroup_AppliesTransformToMatchingZones()
    {
        var def = MinimalBuilder()
            .AddZone("hand", new Dictionary<string, object>())
            .AddZone("deck", new Dictionary<string, object> { ["hidden"] = true })
            .RegisterZoneGroup(
                "tag-visible",
                matcher:   zone => !zone.StaticProperties.ContainsKey("hidden"),
                transform: zone => zone with
                {
                    StaticProperties = new Dictionary<string, object>(zone.StaticProperties) { ["visible"] = true }
                })
            .Build();

        Assert.True(def.ZoneDefinitions["hand"].StaticProperties.TryGetValue("visible", out var v) && v is true);
        Assert.False(def.ZoneDefinitions["deck"].StaticProperties.ContainsKey("visible"));
    }

    // -----------------------------------------------------------------------
    //  Player groups
    // -----------------------------------------------------------------------

    [Fact]
    public void RegisterPlayerGroup_AppliesTransformToMatchingPlayers()
    {
        var def = MinimalBuilder()
            .AddPlayer("hero",    new Dictionary<string, object> { ["role"] = "hero" })
            .AddPlayer("villain", new Dictionary<string, object> { ["role"] = "villain" })
            .RegisterPlayerGroup(
                "hero-bonus",
                matcher:   p => p.StaticProperties.TryGetValue("role", out var r) && r is "hero",
                transform: p => p with
                {
                    StaticProperties = new Dictionary<string, object>(p.StaticProperties) { ["bonus"] = true }
                })
            .Build();

        Assert.True(def.PlayerDefinitions["hero"].StaticProperties.TryGetValue("bonus", out var v) && v is true);
        Assert.False(def.PlayerDefinitions["villain"].StaticProperties.ContainsKey("bonus"));
    }
}
