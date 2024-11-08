using Archetype.Framework.Effects;
using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Archetype.Tests;

public class TestTest
{
    [Test]
    public void Test()
    {
        var atom = Substitute.For<IAtom>();

        var result = AtomicEffect.ChangeStat(atom, "damage", 1);
        
        result.Keyword.Should().Be("ChangeStat");
        result.Results.Should().ContainKey("ChangeStat")
            .WhoseValue.Should().Contain(new AtomicEffect.StatChangeResult("damage", 1));
    }
}