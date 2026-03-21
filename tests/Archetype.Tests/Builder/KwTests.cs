using Archetype.Build;
using Archetype.Core;

namespace Archetype.Tests.Builder;

public class KwTests
{
    // -----------------------------------------------------------------------
    //  Generic node constructors
    // -----------------------------------------------------------------------

    [Fact]
    public void Param_ReturnsParameterRefWithName()
    {
        var node = Kw.Param("target");
        var pr = Assert.IsType<ParameterRef>(node);
        Assert.Equal("target", pr.Name);
    }

    [Fact]
    public void Num_ReturnsLiteralWithDoubleValue()
    {
        var node = Kw.Num(42);
        var lit = Assert.IsType<Literal>(node);
        Assert.Equal(42.0, lit.Value);
    }

    [Fact]
    public void Bool_ReturnsLiteralWithBoolValue()
    {
        var node = Kw.Bool(true);
        var lit = Assert.IsType<Literal>(node);
        Assert.Equal(true, lit.Value);
    }

    [Fact]
    public void Str_ReturnsLiteralWithStringValue()
    {
        var node = Kw.Str("health");
        var lit = Assert.IsType<Literal>(node);
        Assert.Equal("health", lit.Value);
    }

    [Fact]
    public void Invoke_ReturnsInvocationWithKeywordName()
    {
        var node = Kw.Invoke("my-keyword", Kw.Num(1), Kw.Num(2));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("my-keyword", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    // -----------------------------------------------------------------------
    //  Mutation primitives (D12)
    // -----------------------------------------------------------------------

    [Fact]
    public void ModifyAccumulator_ReturnsCorrectInvocation()
    {
        var node = Kw.ModifyAccumulator(Kw.Param("atom"), Kw.Str("hp"), Kw.Num(-3));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("modify-accumulator", inv.KeywordName);
        Assert.Equal(3, inv.Args.Length);
    }

    [Fact]
    public void MoveCard_ReturnsCorrectInvocation()
    {
        var node = Kw.MoveCard(Kw.Param("card"), Kw.Param("zone"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("move-card", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    [Fact]
    public void CreateCard_ReturnsCorrectInvocation()
    {
        var node = Kw.CreateCard(Kw.Param("zone"), Kw.Str("goblin"), Kw.Param("owner"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("create-card", inv.KeywordName);
        Assert.Equal(3, inv.Args.Length);
    }

    // -----------------------------------------------------------------------
    //  Read primitives
    // -----------------------------------------------------------------------

    [Fact]
    public void GetState_ReturnsCorrectInvocation()
    {
        var node = Kw.GetState(Kw.Param("atom"), Kw.Str("health"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("get-state", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    [Fact]
    public void OwnerOf_ReturnsCorrectInvocation()
    {
        var node = Kw.OwnerOf(Kw.Param("card"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("owner-of", inv.KeywordName);
        Assert.Single(inv.Args);
    }

    [Fact]
    public void Session_ReturnsCorrectInvocation()
    {
        var node = Kw.Session();
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("session", inv.KeywordName);
        Assert.Empty(inv.Args);
    }

    // -----------------------------------------------------------------------
    //  Arithmetic
    // -----------------------------------------------------------------------

    [Fact]
    public void Add_ReturnsCorrectInvocation()
    {
        var node = Kw.Add(Kw.Num(1), Kw.Num(2));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("add", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    [Fact]
    public void Max_ReturnsCorrectInvocation()
    {
        var node = Kw.Max(Kw.Num(0), Kw.Param("amount"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("max", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    // -----------------------------------------------------------------------
    //  Boolean / comparison
    // -----------------------------------------------------------------------

    [Fact]
    public void GreaterThan_ReturnsCorrectInvocation()
    {
        var node = Kw.GreaterThan(Kw.Num(5), Kw.Num(3));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("greater-than", inv.KeywordName);
        Assert.Equal(2, inv.Args.Length);
    }

    [Fact]
    public void Not_ReturnsCorrectInvocation()
    {
        var node = Kw.Not(Kw.Bool(true));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("not", inv.KeywordName);
        Assert.Single(inv.Args);
    }

    // -----------------------------------------------------------------------
    //  Game outcome
    // -----------------------------------------------------------------------

    [Fact]
    public void DeclareWinner_ReturnsCorrectInvocation()
    {
        var node = Kw.DeclareWinner(Kw.Param("player"));
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("declare-winner", inv.KeywordName);
        Assert.Single(inv.Args);
    }

    [Fact]
    public void DeclareDraw_ReturnsCorrectInvocation()
    {
        var node = Kw.DeclareDraw();
        var inv = Assert.IsType<Invocation>(node);
        Assert.Equal("declare-draw", inv.KeywordName);
        Assert.Empty(inv.Args);
    }

    // -----------------------------------------------------------------------
    //  Composition produces correct tree structure
    // -----------------------------------------------------------------------

    [Fact]
    public void ComposedExpression_ProducesCorrectTree()
    {
        // max(0, subtract(amount, get-state(target, "defense")))
        var node = Kw.Max(
            Kw.Num(0),
            Kw.Subtract(Kw.Param("amount"), Kw.GetState(Kw.Param("target"), Kw.Str("defense"))));

        var outerMax = Assert.IsType<Invocation>(node);
        Assert.Equal("max", outerMax.KeywordName);

        var subtract = Assert.IsType<Invocation>(outerMax.Args[1]);
        Assert.Equal("subtract", subtract.KeywordName);

        var getState = Assert.IsType<Invocation>(subtract.Args[1]);
        Assert.Equal("get-state", getState.KeywordName);
    }
}
