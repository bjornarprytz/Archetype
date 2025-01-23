using System.Collections;
using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Data;
using Archetype.Framework.Effects;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using Xunit;

namespace Archetype.Tests;

public class ParseTests
{
    private readonly ExpressionParser _sut;
    
    public ParseTests()
    {
        var methods = typeof(TestKeywords).GetMethods().Where(m => m.GetCustomAttribute<EffectAttribute>() != null);
        
        _sut = new ExpressionParser(methods);
    }
    
    
    [Fact]
    public void ParseCard()
    {
        var cardData = BaseCardData with
        {
            Name = "TestCard",
            Targets = new []
            {
                new TargetData()
                {
                    ConditionalExpressions = new []
                    {
                        new ReadExpression("facets:types has 'card'")
                    }
                },
                new TargetData()
                {
                    ConditionalExpressions = new []
                    {
                        new ReadExpression("facets:types has 'card'")
                    }
                }
            },
            Effects = new[]
            {
                new EffectData()
                {
                    Keyword = "Keyword1",
                    ArgumentExpressions = new ReadExpression[]
                    {
                        new ("targets:0"),
                        new ("1")
                    }
                },
                new EffectData()
                {
                    Keyword = "Keyword2",
                    ArgumentExpressions = new ReadExpression[]
                    {
                        new ("targets:1"),
                        new ("state.hand.count")
                    }
                }
            }
        };
        
        var card = _sut.ParseCard(cardData);
        
        card.Name.Should().Be("TestCard");
        card.Targets.Should().BeEquivalentTo(new[]
        {
            new TargetProto()
            {
                Predicates = new []
                {
                    new AtomGroupPredicate<IAtom, string?>(new Group<IAtom, string?>(new [] {"facets:types"}), "has", new ImmediateWord("card"))
                }
            },
            new TargetProto()
            {
                Predicates = new []
                {
                    new AtomGroupPredicate<IAtom, string?>(new Group<IAtom, string?>(new [] {"facets:types"}), "has", new ImmediateWord("card"))
                }
            }
        });
        card.Effects.Should().BeEquivalentTo(
            new []
            {
                new EffectProto()
                {
                    Keyword = "Keyword1",
                    Parameters = new IValue[]
                    {
                        new Value<IResolutionContext, IAtom>(new [] {"targets:0"}),
                        new ImmediateNumber(1)
                    }
                },
                new EffectProto()
                {
                    Keyword = "Keyword2",
                    Parameters = new IValue[]
                    {
                        new Value<IResolutionContext, IAtom>(new [] {"targets:1"}),
                        new Value<IResolutionContext, int?>(new [] {"state", "hand", "count"})
                    }
                }
            }
            );
    }
    
    private static CardData BaseCardData => new()
    {
        Name = "Default",
        Costs = new Dictionary<string, ReadExpression>(),
        Targets = Array.Empty<TargetData>(),
        Stats = new Dictionary<string, ReadExpression>(),
        Facets = new Dictionary<string, string[]>(),
        Tags = Array.Empty<string>(),
        Effects = Array.Empty<EffectData>(),
        Variables = new Dictionary<string, ReadExpression>()
    };
}

internal static class TestKeywords
{
    [Effect("Keyword1")]
    public static IEffectResult Keyword1(IHasStats atom, int arg)
    {
        var current = atom.GetStat("test") ?? 0;
        atom.SetStat("test", current + arg);
        
        return ResultFactory.Atomic(arg - current);
    }
    
    [Effect("Keyword2")]
    public static IEffectResult Keyword2(IHasStats atom, int arg)
    {
        var current = atom.GetStat("test") ?? 0;
        atom.SetStat("test", current - arg);
        
        return ResultFactory.Atomic(arg - current);
    }
}