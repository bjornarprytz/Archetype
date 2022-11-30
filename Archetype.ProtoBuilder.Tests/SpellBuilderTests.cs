using Archetype.Components;
using Archetype.Components.Extensions;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using FluentAssertions;
using NSubstitute;

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

    [Test]
    public void SpellEffectHasCorrectContextualRulesText()
    {
        var contextMock = Substitute.For<IContext<ICard>>();
        contextMock.Target<IUnit>(0).Health.Returns(5);

        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<IUnit>(0).Damage(context.Target<IUnit>(0).Health))
            .Build();
        
        var contextualRulesText = spell.ContextualRulesText(contextMock);
        
        contextualRulesText.Should().Be("Deal {5} damage to this unit.");
    }
    
    [Test]
    public void SpellEffectWithComplexInput_HasCorrectStaticRulesText()
    {
        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<IUnit>(0).Damage(context.Target<IUnit>(0).Health))
            .Build();
        
        var staticRulesText = spell.RulesText;
        
        // TODO: I'm reaching a very low return on complexity with the static rules text. I think I should design a way around this.
        staticRulesText.Should().Be("Deal {X} damage to this unit, where X is the target's health.");
    }
    
    [Test]
    public void SpellEffectWithImmediateInput_HasCorrectStaticRulesText()
    {
        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<IUnit>(0).Damage(69))
            .Build();
        
        var staticRulesText = spell.RulesText;
        
        staticRulesText.Should().Be("Deal {69} damage to this unit.");
    }
    
    [Test]
    public void SpellEffectWithImmediateInput_HasCorrectContextualRulesText()
    {
        var contextMock = Substitute.For<IContext<ICard>>();

        var spell = BuilderFactory.CreateSpellBuilder()
            .AddEffect(context => context.Target<IUnit>(0).Damage(69))
            .Build();
        
        var contextualRulesText = spell.ContextualRulesText(contextMock);
        
        contextualRulesText.Should().Be("Deal {69} damage to this unit.");
    }
    
}