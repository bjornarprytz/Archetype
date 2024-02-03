using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Grammar.Tests;

public class Tests
{
    private CardParser _sut;
    
    [SetUp]
    public void Setup()
    {
        _sut = new CardParser();
    }

    [Test]
    public void LightningBolt()
    {
        const string cardText = """
                                "Lightning Bolt"
                                TYPE: "Spell"
                                {
                                    <T_ANY()>
                                    COST_RESOURCES(1)
                                    DAMAGE(<0> 3)
                                }
                                """;
        var card = _sut.ParseCard(new CardData(cardText, ""));
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        resolutionContext.Targets.Returns(new List<IAtom> { Substitute.For<IAtom>() });
        
        
        card.Should().NotBeNull();
        
        card.Name.Should().Be("Lightning Bolt");
        card.Tags.Should().ContainKey("TYPE").WhoseValue.Should().Be("Spell");
        card.ActionBlock.Costs.ShouldContain("COST_RESOURCES", 1);
        card.ActionBlock.TargetSpecs.ShouldContain("T_ANY");
        card.ActionBlock.Effects.ShouldContain("DAMAGE", resolutionContext, new TargetRef(0), 3);
    }

    [Test]
    public void SeaGateWreckage()
    {
        const string cardText = """
                                "Sea Gate Wreckage"
                                TYPE: "Building"
                                {
                                    COST_RESOURCES(2)
                                }
                                ABILITIES
                                {
                                    "Pilfer" 
                                    {
                                        COST_RESOURCES(1)
                                        DRAW_CARD()
                                    }
                                }
                                """;
        
        var card = _sut.ParseCard(new CardData(cardText, ""));
        
        card.Should().NotBeNull();
        
        card.Name.Should().Be("Sea Gate Wreckage");
        
        card.Tags.Should().ContainKey("TYPE").WhoseValue.Should().Be("Building");
        card.ActionBlock.Costs.ShouldContain("COST_RESOURCES", 2);
        card.Abilities.Should().ContainKey("Pilfer");
        card.Abilities["Pilfer"].Costs.ShouldContain("COST_RESOURCES", 1);
        card.Abilities["Pilfer"].Effects.ShouldContain("DRAW_CARD");
    }

    [Test]
    public void MasterTheWay()
    {
        const string cardText = """
                                "Master the Way"
                                TYPE: "Spell"
                                {
                                     <T_ANY()>
                                     [HAND_SIZE()]
                                     COST_RESOURCES(3)
                                     DRAW_CARD()
                                     DAMAGE(<0> [0])
                                }
                                """;
        
        var card = _sut.ParseCard(new CardData(cardText, ""));
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        resolutionContext.Targets.Returns(new List<IAtom> { Substitute.For<IAtom>() });
        resolutionContext.ComputedValues.Returns(new List<int> { 69 });
        
        card.Should().NotBeNull();
        
        card.Name.Should().Be("Master the Way");
        card.Tags.Should().ContainKey("TYPE").WhoseValue.Should().Be("Spell");
        card.ActionBlock.TargetSpecs.ShouldContain("T_ANY");
        card.ActionBlock.Costs.ShouldContain("COST_RESOURCES", 3);
        card.ActionBlock.Effects.ShouldContain("DRAW_CARD");
        card.ActionBlock.Effects.ShouldContain("DAMAGE", resolutionContext, new TargetRef(0), new ComputeRef(0));
    }

    [Test]
    public void ArcTrail()
    {
        const string cardText = """
                                "Arc Trail"
                                TYPE: "Spell"
                                {
                                     <T_ANY(), T_ANY()>
                                     COST_RESOURCES(2)
                                     DAMAGE(<0> 2)
                                     DAMAGE(<1> 1)
                                }
                                """;
        
        var card = _sut.ParseCard(new CardData(cardText, ""));
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        resolutionContext.Targets.Returns(new List<IAtom> { Substitute.For<IAtom>(), Substitute.For<IAtom>() });
        
        card.Should().NotBeNull();
        card.Name.Should().Be("Arc Trail");
        card.Tags.Should().ContainKey("TYPE").WhoseValue.Should().Be("Spell");
        card.ActionBlock.TargetSpecs.ShouldContain("T_ANY");
        card.ActionBlock.TargetSpecs.Should().HaveCount(2);
        card.ActionBlock.Costs.ShouldContain("COST_RESOURCES", 2);
        card.ActionBlock.Effects.ShouldContain("DAMAGE", resolutionContext, new TargetRef(0), 2);
        card.ActionBlock.Effects.ShouldContain("DAMAGE", resolutionContext, new TargetRef(1), 1);
    }
}