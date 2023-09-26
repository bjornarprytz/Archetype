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
                    (TARGETS <type:any>)

                    (subtype instant)
                    (Color red)
                    (RARITY common)
                    (type spell)

                    (COST_RESOURCE 1)
                    (CONDITION_SELF zone:hand)

                    effects: {
                        (DAMAGE <0> 3)
                    }
                """
        });

        protoCard.Should().NotBeNull();
        protoCard!.Name.Should().Be("Lightning Bolt");

        protoCard.Targets.Should()
            .ContainSingle(c => c.Filters.Count == 1 && c.Filters["type"] == "any" && !c.IsOptional);

        protoCard.Characteristics["SUBTYPE"].Value.Should().Be("instant");
        protoCard.Characteristics["COLOR"].Value.Should().Be("red");
        protoCard.Characteristics["RARITY"].Value.Should().Be("common");
        protoCard.Characteristics["TYPE"].Value.Should().Be("spell");

        protoCard.Costs.Should().ContainSingle(c => (c.Keyword == "COST_RESOURCE" && c.Amount == 1));
        protoCard.Conditions.Should().ContainSingle(c => c.Keyword == "CONDITION_SELF" && c.Operands.Count == 1);

        protoCard.Effects.Should()
            .ContainSingle(c => c.Keyword == "DAMAGE" && c.Targets.Count == 1 && c.Operands.Count == 1);
    }

    [Test]
    public void ArcTrail()
    {
        var definitions = Substitute.For<IDefinitions>();

        var parser = new CardParser(definitions);

        var protoCard = parser.ParseCard(new CardData
        {
            Name = "Arc Trail",
            Text =
                """
                    (TARGETS <type:unit|player?> <type:unit|player?>)
                    (subtype sorcery)
                    (Color red)
                    (RARITY uncommon)
                    (type spell)

                    (COST_RESOURCE 2)
                    (CONDITION_SELF zone:hand)

                    effects: {
                        (DAMAGE <0> 2)
                        (DAMAGE <1> 1)
                    } 
                """
        });
        
        protoCard.Should().NotBeNull();
        protoCard!.Name.Should().Be("Arc Trail");
        
        protoCard.Targets.Should()
            .ContainSingle(c => c.Filters.Count == 1 && c.Filters["type"] == "unit|player?" && !c.IsOptional);
        
        protoCard.Characteristics["SUBTYPE"].Value.Should().Be("sorcery");
        protoCard.Characteristics["COLOR"].Value.Should().Be("red");
        protoCard.Characteristics["RARITY"].Value.Should().Be("uncommon");
        protoCard.Characteristics["TYPE"].Value.Should().Be("spell");
        
        protoCard.Costs.Should().ContainSingle(c => (c.Keyword == "COST_RESOURCE" && c.Amount == 2));
        protoCard.Conditions.Should().ContainSingle(c => c.Keyword == "CONDITION_SELF" && c.Operands.Count == 1);
        
        protoCard.Effects.Should()
            .Contain(c => c.Keyword == "DAMAGE" && c.Targets.Count == 1 && c.Operands.Count == 1);
        
    }
}