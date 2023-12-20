using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Grammar.Tests;

public class Tests
{
    CardParser _sut = new();
    
    // TODO: Create TYPE, COST_MANA, T_ANY, and DAMAGE rules
    
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void Test1()
    {
        const string cardText = """
                                "Lightning Bolt"
                                TYPE: Spell
                                {
                                    COST_MANA(1)
                                    <T_ANY>
                                    DAMAGE(<1> 3)
                                }
                                """;
        var card = _sut.ParseCard(new CardData(cardText, ""));
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        resolutionContext.Targets.Returns(new List<IAtom> { Substitute.For<IAtom>() });
        
        
        card.Should().NotBeNull();
        
        card.Name.Should().Be("Lightning Bolt");
        card.Characteristics.ShouldContain("TYPE", "Spell");
        card.ActionBlock.Costs.ShouldContain("COST_MANA", 1);
        card.ActionBlock.TargetSpecs.ShouldContain("T_ANY");
        card.ActionBlock.Effects.ShouldContain("DAMAGE", resolutionContext, Mock.Operand((c) => c.Targets[0]), 3);
    }
}