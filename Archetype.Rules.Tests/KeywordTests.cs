using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Rules.Extensions;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Rules.Tests;

public class KeywordTests
{
    [Test]
    public void MoveTo_ContentsInBothZonesAreUpdated()
    {
        var card = Substitute.For<ICard>();
        var hand = Substitute.For<IHand>();
        var discardPile = Substitute.For<IDiscardPile>();

        card.CurrentZone.Returns(hand);
        
        card.MoveTo(discardPile);
        ((IZone) hand).Received(1).Remove(card);
        discardPile.Received(1).Add(card);
        card.Received(1).CurrentZone = discardPile;
    }
    
    [Test]
    public void MoveTo_SourceAndDestination_ResultContainsAllData()
    {
        var unit = Substitute.For<IUnit>();
        var node1 = Substitute.For<INode>();
        var node2 = Substitute.For<INode>();
        
        unit.CurrentZone.Returns(node1);
        var result = unit.MoveTo(node2);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Move");
        
        effectResult.Data.Should().ContainKey("Atom")
            .WhoseValue.Should().Be(unit.Id.ToString());
        
        effectResult.Data.Should().ContainKey("Source")
            .WhoseValue.Should().Be(node1.Id.ToString());
        
        effectResult.Data.Should().ContainKey("Destination")
            .WhoseValue.Should().Be(node2.Id.ToString());
    }
    
    [Test]
    public void MoveTo_DestinationButNoSource_ResultContainsAllData()
    {
        var unit = Substitute.For<IUnit>();
        var node1 = Substitute.For<INode>();
        
        unit.CurrentZone.Returns((IZone) null!);
        var result = unit.MoveTo(node1);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Move");
        
        effectResult.Data.Should().ContainKey("Atom")
            .WhoseValue.Should().Be(unit.Id.ToString());
        
        effectResult.Data.Should().ContainKey("Source")
            .WhoseValue.Should().Be("None");
        
        effectResult.Data.Should().ContainKey("Destination")
            .WhoseValue.Should().Be(node1.Id.ToString());
    }
    
    [TestCase(1, 0)]
    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(2, 0)]
    public void Damage_HealthIsReduced_AndResultIsPopulated(int damage, int expectedHealth)
    {
        const int startingHealth = 1;
        
        var unit = Substitute.For<IUnit>();
        unit.CurrentHealth.Returns(startingHealth);
        unit.MaxHealth.Returns(startingHealth);
        
        var result = unit.Damage(damage);
        unit.CurrentHealth.Should().Be(expectedHealth);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Damage");
        
        effectResult.Data.Should().ContainKey("Atom")
            .WhoseValue.Should().Be(unit.Id.ToString());
        
        effectResult.Data.Should().ContainKey("Amount")
            .WhoseValue.Should().Be((startingHealth - expectedHealth).ToString());
    }
    
    [Test]
    public void Damage_ResultContainsAllData()
    {
        const int startingHealth = 1;
        
        var unit = Substitute.For<IUnit>();
        unit.CurrentHealth.Returns(startingHealth);
        unit.MaxHealth.Returns(startingHealth);
        
        var result = unit.Damage(1);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Damage");
        
        
    }
    
    [Test]
    public void Attack_HealthIsReduced()
    {
        const int startingHealth = 1;
        
        var attacker = Substitute.For<IUnit>();
        var defender = Substitute.For<IUnit>();
        
        attacker.CurrentHealth.Returns(startingHealth);
        attacker.MaxHealth.Returns(startingHealth);
        
        attacker.Attack(defender);
        
        defender.CurrentHealth.Should().Be(startingHealth - 1);
    }
    
    [Test]
    public void Attack_ResultIsPopulated()
    {
        var attacker = Substitute.For<IUnit>();
        var target = Substitute.For<IUnit>();
        
        var result = attacker.Attack(target);
        
        result.Results.Should().Contain(effectResult => 
            effectResult.Keyword == "Attack" 
            && effectResult.Data["Attacker"] == attacker.Id.ToString() 
            && effectResult.Data["Target"] == target.Id.ToString());
    }
}