using Archetype.Framework.Definitions;
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
    DAMAGE hand(type:unit|card,trample:true) 4;
    HEAL target 1 "Goodbye";
    DRAW 1;
"""
        });
    }
}