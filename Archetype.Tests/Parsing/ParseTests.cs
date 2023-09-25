using Archetype.Framework.Definitions;
using Archetype.Framework.Parsing;
using Archetype.Framework.Runtime;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.Parsing;

public class ParseTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void LightningBolt()
    {
        var definitions = Substitute.For<IDefinitions>();
        
        var parser = new CardParser(definitions);

        var protoCard = parser.ParseCard(new CardData
        {
            Name = "Lightning Bolt",
            Text = 
"""

(SUBTYPE instant)
(COLOR red)
(RARITY common)
(TYPE spell)
(COST_RESOURCE 1)
(CONDITION_ZONE zone:hand)
(TARGETS type:any)

effects: {
    (DAMAGE <0> 3)
}
"""
        });
        
        protoCard.Should().NotBeNull();
        protoCard.Name.Should().Be("Lightning Bolt");
        protoCard.Characteristics.Should().ContainKey("TYPE").WhoseValue.Should().Be("spell");
        protoCard.Characteristics.Should().ContainKey("SUBTYPE").WhoseValue.Should().Be("instant");
        protoCard.Characteristics.Should().ContainKey("COLOR").WhoseValue.Should().Be("red");
        protoCard.Characteristics.Should().ContainKey("RARITY").WhoseValue.Should().Be("common");
        protoCard.Targets.Should().HaveCount(1);
        protoCard.Targets[0].Filters.Should().ContainKey("TYPE").WhoseValue.Should().Be("any");
        protoCard.Targets[0].IsOptional.Should().BeFalse();
        protoCard.Effects.Should().HaveCount(1);
        protoCard.Effects[0].Keyword.Should().Be("DAMAGE");
        protoCard.Effects[0].Operands.Should().HaveCount(1);
    }

    [Test]
    public void ProdigalSorcerer()
    {
        var definitions = Substitute.For<IDefinitions>();

        var parser = new CardParser(definitions);

        var protoCard = parser.ParseCard(new CardData
        {
            Name = "Prodigal Sorcerer",
            Text =
"""

(TYPE unit)
(SUBTYPE wizard)
(COLOR blue)
(RARITY common)
(CONDITION_ZONE zone:field)
(TARGETS type:any)

effects: {
    (TARGETS type:node)
    (MOVE ~ <0>)
}

abilities: {
    (COST_WORK)
    (TARGETS type:any)
    effects: {
        (DAMAGE <0> 1)
    }
}

"""
        });
        
        protoCard.Should().NotBeNull();
        protoCard.Name.Should().Be("Prodigal Sorcerer");
        protoCard.Characteristics.Should().ContainKey("TYPE").WhoseValue.Should().Be("unit");
        protoCard.Characteristics.Should().ContainKey("SUBTYPE").WhoseValue.Should().Be("wizard");
        protoCard.Characteristics.Should().ContainKey("COLOR").WhoseValue.Should().Be("blue");
        protoCard.Characteristics.Should().ContainKey("RARITY").WhoseValue.Should().Be("common");
        protoCard.Abilities.Should().HaveCount(1);
        protoCard.Abilities.Should().ContainKey("Ping");
        protoCard.Abilities["Ping"].Targets.Should().HaveCount(1);
        protoCard.Abilities["Ping"].Targets[0].Filters.Should().ContainKey("TYPE").WhoseValue.Should().Be("any");
        protoCard.Abilities["Ping"].Targets[0].IsOptional.Should().BeFalse();
        protoCard.Abilities["Ping"].Effects.Should().HaveCount(1);
        protoCard.Abilities["Ping"].Effects[0].Keyword.Should().Be("DAMAGE");
        protoCard.Abilities["Ping"].Effects[0].Operands.Should().HaveCount(1);
        protoCard.Abilities["Ping"].Costs.Should().HaveCount(1);
        protoCard.Abilities["Ping"].Costs[0].Keyword.Should().Be("COST_WORK");
        protoCard.Abilities["Ping"].Costs[0].Operands.Should().HaveCount(1);
    }
}