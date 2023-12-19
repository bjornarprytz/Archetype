using Archetype.Framework.Core.Primitives;
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
        
        card.Should().NotBeNull();
        
        card.Name.Should().Be("Lightning Bolt");
        card.Characteristics.ShouldContain("TYPE", "Spell");
        card.ActionBlock.Costs.ShouldContain("COST_MANA", 1);
        card.ActionBlock.TargetSpecs.ShouldContain("T_ANY");
        card.ActionBlock.Effects.ShouldContain("DAMAGE", "SOMETHING_FOR_TARGET_REF", 3); // TODO: What does a targetRef operand look like?
        
    }
}