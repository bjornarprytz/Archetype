using Archetype.Rules.Extensions;
using Archetype.Rules.State;
using FluentAssertions;

namespace Archetype.Rules.Tests;

public class KeywordTests
{
    [Test]
    public void MoveTo_ContentsInBothZonesAreUpdated()
    {
        var node1 = new Node();
        var node2 = new Node();
        var unit = new Unit();
        
        unit.MoveTo(node1);
        unit.MoveTo(node2);

        node1.Contents.Should().NotContain(unit);
        node2.Contents.Should().Contain(unit);
    }
    
    [Test]
    public void MoveTo_SourceAndDestination_ResultContainsAllData()
    {
        var unit = new Unit();
        var node1 = new Node();
        var node2 = new Node();
        
        unit.MoveTo(node1);
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
        var unit = new Unit();
        var node1 = new Node();
        
        
        var result = unit.MoveTo(node1);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Move");
        
        effectResult.Data.Should().ContainKey("Atom")
            .WhoseValue.Should().Be(unit.Id.ToString());
        
        effectResult.Data.Should().ContainKey("Source")
            .WhoseValue.Should().Be(null);
        
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
        
        var unit = new Unit
        {
            CurrentHealth = startingHealth,
            MaxHealth = startingHealth
        };
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
        var unit = new Unit
        {
            CurrentHealth = 1,
            MaxHealth = 1
        };
        var result = unit.Damage(1);

        var effectResult = result.Results.Single();
        
        effectResult.Keyword.Should().Be("Damage");
        
        
    }
}