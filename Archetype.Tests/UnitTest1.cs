using Archetype.Framework.Parsing;
using Archetype.Framework.Runtime;
using NSubstitute;

namespace Archetype.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var definitions = Substitute.For<IDefinitions>();
        
        var parser = new CardParser(definitions);

        parser.ParseCard(new CardData
        {
            Name = "Lightning Bolt",
            Text = 
""" 
{
    [COMPUTED X:(zone:hand#count)]
    <TARGETS (type:unit|structure)(type:structure)>;
    DAMAGE <0> 4;
    <PROMPT> "Choose a card to discard" 2 (zone:hand);
    DISCARD <1> [;
    DRAW 1;
}
"""
        });
    }
}