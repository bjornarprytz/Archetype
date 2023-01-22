using Archetype.Components;
using Archetype.Components.Extensions;
using Archetype.Core.Atoms.Cards;
using Archetype.Rules.Extensions;
using FluentAssertions;

namespace Archetype.ProtoBuilder.Tests;

public class SpellBuilderTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void SpellWithOneTarget()
    {
        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<ICard>(0).MoveTo(context.Hand()))
            .Build();
        
        spell.TargetDescriptors.Should().ContainSingle(t => t.TargetIndex == 0 && t.TargetType == typeof(ICard));
    }
    
    [Test]
    public void SpellWithTwoTargets()
    {
        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<IUnit>(0).Damage(1))
            .AddEffect(context => context.Target<IUnit>(0).Damage(1))
            .AddEffect(context => context.Target<IUnit>(1).Damage(1))
            .Build();
        
        spell.TargetDescriptors.Should().ContainSingle(t => t.TargetIndex == 0 && t.TargetType == typeof(IUnit));
        spell.TargetDescriptors.Should().ContainSingle(t => t.TargetIndex == 1 && t.TargetType == typeof(IUnit));
    }
}