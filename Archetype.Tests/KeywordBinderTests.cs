using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Effects;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class KeywordBinderTests
{
    private readonly IValue _p1 = Substitute.For<IValue>();
    private readonly IValue _p2 = Substitute.For<IValue>();
    private readonly IValue _p3 = Substitute.For<IValue>();
    private readonly KeywordBinder _sut = new(typeof(KeywordBinderTests).GetMethod(nameof(TestKeyword1))!);

    [Fact]
    public void BindKeyword()
    {
        
        _p1.GetValue(default!).ReturnsForAnyArgs("someWord");
        _p2.GetValue(default!).ReturnsForAnyArgs(1);
        _p3.GetValue(default!).ReturnsForAnyArgs(Substitute.For<IAtom>());
        
        var effectProto = new EffectProto
        {
            Keyword = "TestKeyword1",
            Parameters = new [] { _p1, _p2, _p3 }
        };

        var act = () => _sut.BindResolver(effectProto);
        
        act.Should().NotThrow();
    }
    
    [Fact]
    public void ResolveKeyword()
    {
        var target = Substitute.For<IAtom>();
        
        _p1.GetValue(default!).ReturnsForAnyArgs("someWord");
        _p2.GetValue(default!).ReturnsForAnyArgs(1);
        _p3.GetValue(default!).ReturnsForAnyArgs(target);
        
        var effectProto = new EffectProto
        {
            Keyword = "TestKeyword1",
            Parameters = new [] { _p1, _p2, _p3 }
        };

        var resolver = _sut.BindResolver(effectProto);

        var result = resolver.Invoke(Substitute.For<IResolutionContext>());
        
        result.Result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("TestKeyword1", new TestKeyword1Result("someWord", 1, target)));
    }

    public record TestKeyword1Result(string Stat, int Change, IAtom Target);
    
    [Effect("TestKeyword1")]
    public static IEffectResult TestKeyword1(string stat, int change, IAtom target)
    {
        return new AtomicResult("TestKeyword1", new TestKeyword1Result(stat, change, target));
    }  
}