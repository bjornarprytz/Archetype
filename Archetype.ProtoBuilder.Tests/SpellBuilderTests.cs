using Archetype.Components;
using Archetype.Components.Extensions;
using Archetype.Core.Atoms;
using FluentAssertions;

namespace Archetype.ProtoBuilder.Tests;

public class SpellBuilderTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void Test1()
    {
        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<ICard>(0).MoveTo(context.Hand()))
            .Build();
        
        spell.TargetDescriptors.Should().ContainSingle(t => t.TargetIndex == 0 && t.TargetType == typeof(ICard));
        
    }
}