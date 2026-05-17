using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Build;
using Archetype.Core;
using Xunit;

namespace Archetype.Tests.Builder;

public class AtomGroupTests
{
    [Fact]
    public void AtomGroup_DefaultCost_AppliesToCardMissingCost()
    {
        var builder = new GameDefinitionBuilder()
            .WithId("atomgroup-test")
            .WithInitManifest(InitManifest.Empty);

        var card = new CardDefinition(
            Name: "cheapling",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: EffectBlockDef.Empty,
            AdditionalEffects: Array.Empty<NamedEffectBlockDef>(),
            StaticEffects: Array.Empty<StaticEffectDef>(),
            ActivationCondition: null,
            Cost: null,
            StateMapDeclarations: null);

        builder.AddCard(card);

        builder.RegisterAtomGroup(new AtomGroup(
            name: "default-cost",
            kinds: new[] { AtomKind.Card },
            cardMatcher: (n, c) => true,
            cardTransform: c => c with { Cost = new CostDef[] { new CostDef(EffectBlockDef.Empty, Array.Empty<ParameterDecl>()) } },
            priority: 0));

        var def = builder.Build();
        Assert.True(def.CardDefinitions.ContainsKey("cheapling"));
        var builtCard = def.CardDefinitions["cheapling"];
        Assert.NotNull(builtCard.Cost);
        Assert.Single(builtCard.Cost);
    }

    [Fact]
    public void AtomGroup_DoesNotOverrideLocalCostByDefault()
    {
        var builder = new GameDefinitionBuilder()
            .WithId("atomgroup-test-2")
            .WithInitManifest(InitManifest.Empty);

        var originalCost = new CostDef[] { new CostDef(EffectBlockDef.Empty, Array.Empty<ParameterDecl>()) };

        var card = new CardDefinition(
            Name: "sturdy",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: EffectBlockDef.Empty,
            AdditionalEffects: Array.Empty<NamedEffectBlockDef>(),
            StaticEffects: Array.Empty<StaticEffectDef>(),
            ActivationCondition: null,
            Cost: originalCost,
            StateMapDeclarations: null);

        builder.AddCard(card);

        builder.RegisterAtomGroup(new AtomGroup(
            name: "default-cost",
            kinds: new[] { AtomKind.Card },
            cardMatcher: (n, c) => true,
            cardTransform: c => c with { Cost = new CostDef[] { new CostDef(EffectBlockDef.Empty, new[] { new ParameterDecl("x", TypeName.Number) }) } },
            priority: 0));

        var def = builder.Build();
        var builtCard = def.CardDefinitions["sturdy"];
        Assert.NotNull(builtCard.Cost);
        Assert.Single(builtCard.Cost);
        Assert.Empty(builtCard.Cost[0].Parameters);
    }

    [Fact]
    public void AtomGroup_OverrideLocal_AllowsOverwrite()
    {
        var builder = new GameDefinitionBuilder()
            .WithId("atomgroup-test-3")
            .WithInitManifest(InitManifest.Empty);

        var originalCost = new CostDef[] { new CostDef(EffectBlockDef.Empty, Array.Empty<ParameterDecl>()) };

        var card = new CardDefinition(
            Name: "fragile",
            StaticProperties: new Dictionary<string, object>(),
            PrimaryEffect: EffectBlockDef.Empty,
            AdditionalEffects: Array.Empty<NamedEffectBlockDef>(),
            StaticEffects: Array.Empty<StaticEffectDef>(),
            ActivationCondition: null,
            Cost: originalCost,
            StateMapDeclarations: null);

        builder.AddCard(card);

        builder.RegisterAtomGroup(new AtomGroup(
            name: "override-cost",
            kinds: new[] { AtomKind.Card },
            cardMatcher: (n, c) => true,
            cardTransform: c => c with { Cost = new CostDef[] { new CostDef(EffectBlockDef.Empty, new[] { new ParameterDecl("x", TypeName.Number) }) } },
            priority: 0,
            overrideLocal: true));

        var def = builder.Build();
        var builtCard = def.CardDefinitions["fragile"];
        Assert.NotNull(builtCard.Cost);
        Assert.Single(builtCard.Cost);
        Assert.Single(builtCard.Cost[0].Parameters);
    }
}