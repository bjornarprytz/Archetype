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
                    (STATIC 
                        (subtype instant)
                        (Color red)
                        (RARITY common)
                        (type spell)
                        (TRAMPLE)
                    )

                    (EFFECTS {
                        (COSTS
                            (COST_RESOURCE 1)
                        )
                        (CONDITIONS
                            (CONDITION_SELF zone:hand)
                        )
                        (TARGETS <type:any>)
                        (DAMAGE <0> 3)
                    })
                """
        });

        protoCard.Should().NotBeNull();
        protoCard!.Name.Should().Be("Lightning Bolt");

        protoCard.Characteristics["SUBTYPE"].Operands[0].GetValue(null).Should().Be("instant");
        protoCard.Characteristics["COLOR"].Operands[0].GetValue(null).Should().Be("red");
        protoCard.Characteristics["RARITY"].Operands[0].GetValue(null).Should().Be("common");
        protoCard.Characteristics["TYPE"].Operands[0].GetValue(null).Should().Be("spell");
        protoCard.Characteristics["TRAMPLE"].Operands[0].GetValue(null).Should().Be("true"); // TODO: Should this be a bool?

        protoCard.Costs.Should().ContainSingle(c => (c.Keyword == "COST_RESOURCE" && c.Operands[0].GetValue(null).Equals(1)));
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
                    (STATIC
                        (subtype sorcery)
                        (Color red)
                        (RARITY uncommon)
                        (type spell)
                    )


                    (EFFECTS {
                        (COSTS
                            (COST_RESOURCE 2)
                        )
                        (CONDITIONS
                            (CONDITION_SELF zone:hand)
                        )
                        (TARGETS <type:any?> <type:unit|player?>)
                        (DAMAGE <0> 2)
                        (DAMAGE <1> 1)
                    })
                """
        });
        
        protoCard.Should().NotBeNull();
        protoCard!.Name.Should().Be("Arc Trail");

        protoCard.Targets.Should().HaveCount(2);
        protoCard.Targets[0].Filter.Should().BeEquivalentTo(Filter.Parse("<type:any?>"));
        protoCard.Targets[1].Filter.Should().BeEquivalentTo(Filter.Parse("<type:unit|player?>"));
            
        
        protoCard.Characteristics["SUBTYPE"].Operands[0].GetValue(null).Should().Be("sorcery");
        protoCard.Characteristics["COLOR"].Operands[0].GetValue(null).Should().Be("red");
        protoCard.Characteristics["RARITY"].Operands[0].GetValue(null).Should().Be("uncommon");
        protoCard.Characteristics["TYPE"].Operands[0].GetValue(null).Should().Be("spell");
        
        protoCard.Costs.Should().ContainSingle(c => (c.Keyword == "COST_RESOURCE" && c.Operands[0].GetValue(null).Equals(2)));
        protoCard.Conditions.Should().ContainSingle(c => c.Keyword == "CONDITION_SELF" && c.Operands.Count == 1);
        
        protoCard.Effects.Should()
            .Contain(c => c.Keyword == "DAMAGE" && c.Targets.Count == 1 && c.Operands.Count == 1);
        
    }
}