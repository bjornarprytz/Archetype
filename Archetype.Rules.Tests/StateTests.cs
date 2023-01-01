using Archetype.Rules.Extensions;
using Archetype.Rules.State;
using FluentAssertions;

namespace Archetype.Rules.Tests;

public class Tests
{
    private DrawPile _drawPile;
    [SetUp]
    public void Setup()
    {
        _drawPile = new DrawPile(new Random(69));
    }

    [Test]
    public void DrawPile_IsEmpty_PeekTopCardReturnsNull()
    {
        _drawPile.PeekTopCard().Should().BeNull();
    }
    
    [Test]
    public void DrawPile_LastCardRemoved_PeekTopCardReturnsNull()
    {
        var card = new Card();
        
        card.MoveTo(_drawPile);
        card.MoveTo(new Hand());
        
        _drawPile.PeekTopCard().Should().BeNull();
    }
    
    [Test]
    public void DrawPile_OneCard_PeekTopCardReturnsIt()
    {
        var card = new Card();
        
        card.MoveTo(_drawPile);
        
        _drawPile.PeekTopCard().Should().Be(card);
    }
    
    [Test]
    public void DrawPile_TwoCards_ReturnsTopCard()
    {
        var card = new Card();
        var card2 = new Card();
        
        card.MoveTo(_drawPile);
        card2.MoveTo(_drawPile);
        
        _drawPile.PeekTopCard().Should().Be(card2);
    }
}