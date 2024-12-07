using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class AtomPredicateTests
{
    private readonly IResolutionContext _context = Substitute.For<IResolutionContext>();
    private readonly ICard _card = Substitute.For<ICard>();
    
    private static readonly string[] Types = new [] { "card" };
    private const int Strength = 2;
    private const string Name = "TestCard";
    
    public AtomPredicateTests()
    {
        _card.GetFacet("types").Returns(Types);
        _card.GetStat("strength").Returns(Strength);
        _card.GetName().Returns(Name);
    }
    
    [Theory]
    [InlineData("has", true)]
    [InlineData("!has", false)]
    public void CollectionContains(string compareOperator, bool expectedResult)
    {
        var atomValue = new AtomGroup<string>("facets:types".Split('.'));
        var compareValue = new ImmediateWord("card");

        var predicate = new AtomGroupPredicate<string?>(atomValue, compareOperator, compareValue);
        
        var result = predicate.Evaluate(_context, _card);
        
        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(Strength, "=",  true)]
    [InlineData(Strength,"!=", false)]
    [InlineData( Strength-1, "!=", true)]
    
    [InlineData(Strength, ">", false)]
    [InlineData(Strength-1, ">", true)]
    
    [InlineData( Strength, "<", false)]
    [InlineData(Strength+1, "<", true)]
    
    [InlineData(Strength, ">=", true)]
    [InlineData(Strength+1, ">=", false)]
    
    [InlineData(Strength, "<=", true)]
    [InlineData(Strength-1, "<=", false)]
    public void CompareNumbers(int value, string compareOperand, bool expectedResult)
    {
        var atomValue = new AtomValue<int?>("stats:strength".Split('.'));
        var compareValue = new ImmediateNumber(value);

        var predicate = new AtomPredicate<int?>(atomValue, compareOperand, compareValue);
        
        var result = predicate.Evaluate(_context, _card);
        
        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(Name, "=", true)]
    [InlineData(Name, "!=", false)]
    [InlineData("NotTestCard", "=", false)]
    [InlineData("NotTestCard", "!=", true)]
    public void CompareStrings(string value, string compareOperand, bool expectedResult)
    {
        var atomValue = new AtomWord("name".Split('.')); // TODO: It's apparent that because name is not present in atom, but in card, the test will fail. Will need to fix this.
        var compareValue = new ImmediateWord(value);

        var predicate = new AtomPredicate<string?>(atomValue, compareOperand, compareValue);
        
        var result = predicate.Evaluate(_context, _card);
        
        result.Should().Be(expectedResult);
    }
}