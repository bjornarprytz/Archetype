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

    
}