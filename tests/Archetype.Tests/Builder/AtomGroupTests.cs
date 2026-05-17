using System;
using System.Collections.Generic;
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
}